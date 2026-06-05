namespace My.Talli.Web.Endpoints;

using Domain.Commands.Billing;
using Domain.Commands.Platforms;
using Domain.Components;
using Domain.Framework;
using Domain.Repositories;
using Microsoft.AspNetCore.DataProtection;
using System.Text.Json;
using Web.Services.Platforms;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Endpoint</summary>
public static class PlatformEndpoints
{
    #region <Constants>

    private const string EtsyChallengeCookieName = "mt_etsy_oauth";
    private const string EtsyChallengePurpose = "Etsy.OAuth.v1";
    private const int EtsyRefreshTokenLifetimeDays = 90;
    private const string GumroadChallengeCookieName = "mt_gumroad_oauth";
    private const string GumroadChallengePurpose = "Gumroad.OAuth.v1";
    private const string StripeChallengeCookieName = "mt_stripe_oauth";
    private const string StripeChallengePurpose = "Stripe.OAuth.v1";

    #endregion

    #region <Endpoints>

    public static void MapPlatformEndpoints(this IEndpointRouteBuilder app)
    {
        // Etsy
        app.MapGet("/api/platforms/etsy/connect", EtsyConnect).RequireAuthorization();
        app.MapGet("/api/platforms/etsy/reconnect/{shopConnectionId:long}", EtsyReconnect).RequireAuthorization();
        app.MapGet("/api/platforms/etsy/callback", EtsyCallback).RequireAuthorization();

        // Gumroad
        app.MapGet("/api/platforms/gumroad/connect", GumroadConnect).RequireAuthorization();
        app.MapGet("/api/platforms/gumroad/reconnect/{shopConnectionId:long}", GumroadReconnect).RequireAuthorization();
        app.MapGet("/api/platforms/gumroad/callback", GumroadCallback).RequireAuthorization();

        // Stripe
        app.MapGet("/api/platforms/stripe/connect", StripeConnect).RequireAuthorization();
        app.MapGet("/api/platforms/stripe/reconnect/{shopConnectionId:long}", StripeReconnect).RequireAuthorization();
        app.MapGet("/api/platforms/stripe/callback", StripeCallback).RequireAuthorization();
    }

    #endregion

    #region <Methods>

    private static async Task<IResult> EtsyConnect(
        HttpContext context,
        EtsyService etsy,
        IDataProtectionProvider dataProtectionProvider,
        CanConnectAnotherShopCommand canConnectAnotherShop)
    {
        var userIdClaim = context.User.FindFirst("UserId")?.Value;
        if (!long.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();

        if (!await canConnectAnotherShop.ExecuteAsync(userId))
            return Results.Redirect("/platforms?error=plan_limit");

        var challenge = etsy.BuildAuthorizeChallenge();
        var protector = dataProtectionProvider.CreateProtector(EtsyChallengePurpose);
        var cookiePayload = JsonSerializer.Serialize(new EtsyOAuthCookie { CodeVerifier = challenge.CodeVerifier, State = challenge.State, UserId = userId });
        var protectedPayload = protector.Protect(cookiePayload);

        context.Response.Cookies.Append(EtsyChallengeCookieName, protectedPayload, ToCookieOptions(context));

        return Results.Redirect(challenge.AuthorizeUrl);
    }

    private static async Task<IResult> EtsyCallback(HttpContext context, EtsyService etsy, IDataProtectionProvider dataProtectionProvider, ConnectEtsyCommand connectEtsy, SwapShopAccountCommand swapShop, ILogger<Program> logger)
    {
        var error = context.Request.Query["error"].ToString();
        if (!string.IsNullOrEmpty(error))
        {
            logger.LogWarning("Etsy OAuth returned error: {Error}", error);
            ClearCookie(context, EtsyChallengeCookieName);
            return Results.Redirect("/platforms?error=etsy_denied");
        }

        var code = context.Request.Query["code"].ToString();
        var state = context.Request.Query["state"].ToString();
        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
        {
            ClearCookie(context, EtsyChallengeCookieName);
            return Results.Redirect("/platforms?error=etsy_invalid");
        }

        if (!context.Request.Cookies.TryGetValue(EtsyChallengeCookieName, out var protectedPayload) || string.IsNullOrEmpty(protectedPayload))
            return Results.Redirect("/platforms?error=etsy_expired");

        EtsyOAuthCookie? cookie;
        try
        {
            var protector = dataProtectionProvider.CreateProtector(EtsyChallengePurpose);
            var cookieJson = protector.Unprotect(protectedPayload);
            cookie = JsonSerializer.Deserialize<EtsyOAuthCookie>(cookieJson);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Etsy OAuth cookie could not be unprotected");
            ClearCookie(context, EtsyChallengeCookieName);
            return Results.Redirect("/platforms?error=etsy_expired");
        }

        if (cookie is null || cookie.State != state)
        {
            logger.LogWarning("Etsy OAuth state mismatch for user {UserId}", cookie?.UserId);
            ClearCookie(context, EtsyChallengeCookieName);
            return Results.Redirect("/platforms?error=etsy_state");
        }

        try
        {
            var tokenResponse = await etsy.ExchangeCodeAsync(code, cookie.CodeVerifier);
            var platformAccountId = EtsyPkceGenerator.ExtractEtsyUserId(tokenResponse.AccessToken);
            var shops = await etsy.GetShopsAsync(platformAccountId, tokenResponse.AccessToken);

            // Reconnect flow — re-point the existing shop to whichever account the user just authorized.
            if (cookie.ReconnectShopConnectionId is long reconnectShopId)
                return await SwapEtsyAsync(context, swapShop, cookie.UserId, reconnectShopId, tokenResponse, platformAccountId, shops, logger);

            // TRANSACTION
            var result = await EnforcedTransactionScope.ExecuteAsync(async () => await connectEtsy.ExecuteAsync(cookie.UserId, tokenResponse, platformAccountId, shops));

            logger.LogInformation("Etsy OAuth connected for user {UserId}: firstConnection={IsFirst} new={NewShopCount} refreshed={RefreshedShopCount}", cookie.UserId, result.IsFirstConnection, result.NewShopCount, result.RefreshedShopCount);
            ClearCookie(context, EtsyChallengeCookieName);

            var status = (result.IsFirstConnection, result.NewShopCount) switch
            {
                (true, _) => "connected",
                (false, > 0) => "added",
                _ => "refreshed"
            };
            return Results.Redirect($"/platforms?etsy={status}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Etsy OAuth token exchange failed for user {UserId}", cookie.UserId);
            ClearCookie(context, EtsyChallengeCookieName);
            return Results.Redirect("/platforms?error=etsy_exchange");
        }
    }

    private static async Task<IResult> EtsyReconnect(
        long shopConnectionId,
        HttpContext context,
        EtsyService etsy,
        IDataProtectionProvider dataProtectionProvider,
        RepositoryAdapterAsync<MODELS.ShopConnection, ENTITIES.ShopConnection> shopConnectionAdapter)
    {
        var userIdClaim = context.User.FindFirst("UserId")?.Value;
        if (!long.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();

        // Ownership check — skip the plan-limit check (the user already owns this shop, this is a refresh-tokens flow not an add-shop flow)
        var shop = (await shopConnectionAdapter.FindAsync(s =>
            s.Id == shopConnectionId &&
            s.UserId == userId &&
            s.PlatformConnection.Platform == "Etsy")).FirstOrDefault();

        if (shop is null)
            return Results.Redirect("/platforms?error=etsy_reconnect_notfound");

        var challenge = etsy.BuildAuthorizeChallenge();
        var protector = dataProtectionProvider.CreateProtector(EtsyChallengePurpose);
        var cookiePayload = JsonSerializer.Serialize(new EtsyOAuthCookie { CodeVerifier = challenge.CodeVerifier, ReconnectShopConnectionId = shopConnectionId, State = challenge.State, UserId = userId });
        var protectedPayload = protector.Protect(cookiePayload);

        context.Response.Cookies.Append(EtsyChallengeCookieName, protectedPayload, ToCookieOptions(context));

        return Results.Redirect(challenge.AuthorizeUrl);
    }

    private static async Task<IResult> GumroadConnect(
        HttpContext context,
        GumroadService gumroad,
        IDataProtectionProvider dataProtectionProvider,
        CanConnectAnotherShopCommand canConnectAnotherShop)
    {
        var userIdClaim = context.User.FindFirst("UserId")?.Value;
        if (!long.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();

        if (!await canConnectAnotherShop.ExecuteAsync(userId))
            return Results.Redirect("/platforms?error=plan_limit");

        var challenge = gumroad.BuildAuthorizeChallenge();
        var protector = dataProtectionProvider.CreateProtector(GumroadChallengePurpose);
        var cookiePayload = JsonSerializer.Serialize(new GumroadOAuthCookie { State = challenge.State, UserId = userId });
        var protectedPayload = protector.Protect(cookiePayload);

        context.Response.Cookies.Append(GumroadChallengeCookieName, protectedPayload, ToCookieOptions(context));

        return Results.Redirect(challenge.AuthorizeUrl);
    }

    private static async Task<IResult> GumroadCallback(HttpContext context, GumroadService gumroad, IDataProtectionProvider dataProtectionProvider, ConnectGumroadCommand connectGumroad, SwapShopAccountCommand swapShop, ILogger<Program> logger, CancellationToken cancellationToken)
    {
        var error = context.Request.Query["error"].ToString();
        if (!string.IsNullOrEmpty(error))
        {
            logger.LogWarning("Gumroad OAuth returned error: {Error}", error);
            ClearCookie(context, GumroadChallengeCookieName);
            return Results.Redirect("/platforms?error=gumroad_denied");
        }

        var code = context.Request.Query["code"].ToString();
        var state = context.Request.Query["state"].ToString();
        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
        {
            ClearCookie(context, GumroadChallengeCookieName);
            return Results.Redirect("/platforms?error=gumroad_invalid");
        }

        if (!context.Request.Cookies.TryGetValue(GumroadChallengeCookieName, out var protectedPayload) || string.IsNullOrEmpty(protectedPayload))
            return Results.Redirect("/platforms?error=gumroad_expired");

        GumroadOAuthCookie? cookie;
        try
        {
            var protector = dataProtectionProvider.CreateProtector(GumroadChallengePurpose);
            var cookieJson = protector.Unprotect(protectedPayload);
            cookie = JsonSerializer.Deserialize<GumroadOAuthCookie>(cookieJson);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Gumroad OAuth cookie could not be unprotected");
            ClearCookie(context, GumroadChallengeCookieName);
            return Results.Redirect("/platforms?error=gumroad_expired");
        }

        if (cookie is null || cookie.State != state)
        {
            logger.LogWarning("Gumroad OAuth state mismatch for user {UserId}", cookie?.UserId);
            ClearCookie(context, GumroadChallengeCookieName);
            return Results.Redirect("/platforms?error=gumroad_state");
        }

        try
        {
            var tokenResponse = await gumroad.ExchangeCodeAsync(code, cancellationToken);
            var user = await gumroad.GetUserAsync(tokenResponse.AccessToken, cancellationToken);
            if (user is null)
            {
                logger.LogWarning("Gumroad user fetch returned null for user {UserId}", cookie.UserId);
                ClearCookie(context, GumroadChallengeCookieName);
                return Results.Redirect("/platforms?error=gumroad_exchange");
            }

            // Reconnect flow — re-point the existing shop to whichever account the user just authorized.
            if (cookie.ReconnectShopConnectionId is long reconnectShopId)
                return await SwapGumroadAsync(context, swapShop, cookie.UserId, reconnectShopId, tokenResponse, user);

            // TRANSACTION
            var result = await EnforcedTransactionScope.ExecuteAsync(async () => await connectGumroad.ExecuteAsync(cookie.UserId, tokenResponse, user));

            logger.LogInformation("Gumroad OAuth connected for user {UserId}: firstConnection={IsFirst} newShop={WasNewShop}", cookie.UserId, result.IsFirstConnection, result.WasNewShop);
            ClearCookie(context, GumroadChallengeCookieName);

            var status = (result.IsFirstConnection, result.WasNewShop) switch
            {
                (true, _) => "connected",
                (false, true) => "added",
                _ => "refreshed"
            };
            return Results.Redirect($"/platforms?gumroad={status}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Gumroad OAuth token exchange failed for user {UserId}", cookie.UserId);
            ClearCookie(context, GumroadChallengeCookieName);
            return Results.Redirect("/platforms?error=gumroad_exchange");
        }
    }

    private static async Task<IResult> GumroadReconnect(
        long shopConnectionId,
        HttpContext context,
        GumroadService gumroad,
        IDataProtectionProvider dataProtectionProvider,
        RepositoryAdapterAsync<MODELS.ShopConnection, ENTITIES.ShopConnection> shopConnectionAdapter)
    {
        var userIdClaim = context.User.FindFirst("UserId")?.Value;
        if (!long.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();

        // Ownership check — skip the plan-limit check (refresh-tokens flow, not add-shop flow)
        var shop = (await shopConnectionAdapter.FindAsync(s =>
            s.Id == shopConnectionId &&
            s.UserId == userId &&
            s.PlatformConnection.Platform == "Gumroad")).FirstOrDefault();

        if (shop is null)
            return Results.Redirect("/platforms?error=gumroad_reconnect_notfound");

        var challenge = gumroad.BuildAuthorizeChallenge();
        var protector = dataProtectionProvider.CreateProtector(GumroadChallengePurpose);
        var cookiePayload = JsonSerializer.Serialize(new GumroadOAuthCookie { ReconnectShopConnectionId = shopConnectionId, State = challenge.State, UserId = userId });
        var protectedPayload = protector.Protect(cookiePayload);

        context.Response.Cookies.Append(GumroadChallengeCookieName, protectedPayload, ToCookieOptions(context));

        return Results.Redirect(challenge.AuthorizeUrl);
    }

    private static async Task<IResult> StripeConnect(
        HttpContext context,
        StripeConnectService stripeConnect,
        IDataProtectionProvider dataProtectionProvider,
        CanConnectAnotherShopCommand canConnectAnotherShop)
    {
        var userIdClaim = context.User.FindFirst("UserId")?.Value;
        if (!long.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();

        if (!await canConnectAnotherShop.ExecuteAsync(userId))
            return Results.Redirect("/platforms?error=plan_limit");

        var challenge = stripeConnect.BuildAuthorizeChallenge();
        var protector = dataProtectionProvider.CreateProtector(StripeChallengePurpose);
        var cookiePayload = JsonSerializer.Serialize(new StripeOAuthCookie { State = challenge.State, UserId = userId });
        var protectedPayload = protector.Protect(cookiePayload);

        context.Response.Cookies.Append(StripeChallengeCookieName, protectedPayload, ToCookieOptions(context));

        return Results.Redirect(challenge.AuthorizeUrl);
    }

    private static async Task<IResult> StripeReconnect(
        long shopConnectionId,
        HttpContext context,
        StripeConnectService stripeConnect,
        IDataProtectionProvider dataProtectionProvider,
        RepositoryAdapterAsync<MODELS.ShopConnection, ENTITIES.ShopConnection> shopConnectionAdapter)
    {
        var userIdClaim = context.User.FindFirst("UserId")?.Value;
        if (!long.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();

        // Ownership check — skip the plan-limit check (refresh-tokens flow, not add-shop flow)
        var shop = (await shopConnectionAdapter.FindAsync(s =>
            s.Id == shopConnectionId &&
            s.UserId == userId &&
            s.PlatformConnection.Platform == "Stripe")).FirstOrDefault();

        if (shop is null)
            return Results.Redirect("/platforms?error=stripe_reconnect_notfound");

        var challenge = stripeConnect.BuildAuthorizeChallenge();
        var protector = dataProtectionProvider.CreateProtector(StripeChallengePurpose);
        var cookiePayload = JsonSerializer.Serialize(new StripeOAuthCookie { ReconnectShopConnectionId = shopConnectionId, State = challenge.State, UserId = userId });
        var protectedPayload = protector.Protect(cookiePayload);

        context.Response.Cookies.Append(StripeChallengeCookieName, protectedPayload, ToCookieOptions(context));

        return Results.Redirect(challenge.AuthorizeUrl);
    }

    private static async Task<IResult> StripeCallback(HttpContext context, StripeConnectService stripeConnect, IDataProtectionProvider dataProtectionProvider, ConnectStripeCommand connectStripe, SwapShopAccountCommand swapShop, ILogger<Program> logger, CancellationToken cancellationToken)
    {
        var error = context.Request.Query["error"].ToString();
        if (!string.IsNullOrEmpty(error))
        {
            logger.LogWarning("Stripe OAuth returned error: {Error}", error);
            ClearCookie(context, StripeChallengeCookieName);
            return Results.Redirect("/platforms?error=stripe_denied");
        }

        var code = context.Request.Query["code"].ToString();
        var state = context.Request.Query["state"].ToString();
        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
        {
            ClearCookie(context, StripeChallengeCookieName);
            return Results.Redirect("/platforms?error=stripe_invalid");
        }

        if (!context.Request.Cookies.TryGetValue(StripeChallengeCookieName, out var protectedPayload) || string.IsNullOrEmpty(protectedPayload))
            return Results.Redirect("/platforms?error=stripe_expired");

        StripeOAuthCookie? cookie;
        try
        {
            var protector = dataProtectionProvider.CreateProtector(StripeChallengePurpose);
            var cookieJson = protector.Unprotect(protectedPayload);
            cookie = JsonSerializer.Deserialize<StripeOAuthCookie>(cookieJson);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Stripe OAuth cookie could not be unprotected");
            ClearCookie(context, StripeChallengeCookieName);
            return Results.Redirect("/platforms?error=stripe_expired");
        }

        if (cookie is null || cookie.State != state)
        {
            logger.LogWarning("Stripe OAuth state mismatch for user {UserId}", cookie?.UserId);
            ClearCookie(context, StripeChallengeCookieName);
            return Results.Redirect("/platforms?error=stripe_state");
        }

        try
        {
            var tokenResponse = await stripeConnect.ExchangeCodeAsync(code, cancellationToken);
            var account = await stripeConnect.GetAccountAsync(tokenResponse.AccessToken, tokenResponse.StripeUserId, cancellationToken);
            var accountInfo = ToAccountInfo(account, tokenResponse.StripeUserId);

            // Reconnect flow — re-point the existing shop to whichever account the user just authorized.
            if (cookie.ReconnectShopConnectionId is long reconnectShopId)
                return await SwapStripeAsync(context, swapShop, cookie.UserId, reconnectShopId, accountInfo, tokenResponse.AccessToken, tokenResponse.RefreshToken);

            // TRANSACTION
            var result = await EnforcedTransactionScope.ExecuteAsync(async () => await connectStripe.ExecuteAsync(cookie.UserId, accountInfo, tokenResponse.AccessToken, tokenResponse.RefreshToken));

            logger.LogInformation("Stripe OAuth connected for user {UserId}: account={AccountId} firstConnection={IsFirst} newShop={WasNewShop}", cookie.UserId, accountInfo.AccountId, result.IsFirstConnection, result.WasNewShop);
            ClearCookie(context, StripeChallengeCookieName);

            var status = (result.IsFirstConnection, result.WasNewShop) switch
            {
                (true, _) => "connected",
                (false, true) => "added",
                _ => "refreshed"
            };
            return Results.Redirect($"/platforms?stripe={status}");
        }
        catch (Exception ex)
        {
            // Diagnostic belt-and-suspenders: also write to stderr so the exception is visible in Azure Log Stream
            // regardless of ILogger configuration / filter weirdness. This catch block swallows the exception
            // for a clean user redirect, which means Elmah never sees it either — stderr is our only direct channel.
            Console.Error.WriteLine($"[STRIPE_OAUTH_FAIL] User={cookie.UserId} ExceptionType={ex.GetType().FullName} Message={ex.Message}");
            Console.Error.WriteLine(ex.ToString());

            logger.LogError(ex, "Stripe OAuth token exchange failed for user {UserId}", cookie.UserId);
            ClearCookie(context, StripeChallengeCookieName);
            return Results.Redirect("/platforms?error=stripe_exchange");
        }
    }

    private static async Task<IResult> SwapEtsyAsync(HttpContext context, SwapShopAccountCommand swapShop, long userId, long shopConnectionId, EtsyTokenResponse tokens, string platformAccountId, IReadOnlyList<EtsyShop> shops, ILogger logger)
    {
        var shop = shops.FirstOrDefault();
        if (shop is null)
        {
            logger.LogWarning("Etsy reconnect returned no shops for user {UserId}", userId);
            ClearCookie(context, EtsyChallengeCookieName);
            return Results.Redirect("/platforms?error=etsy_exchange");
        }

        var now = DateTime.UtcNow;
        var input = new SwapShopAccountInput
        {
            AccessToken = tokens.AccessToken,
            EtsyDetail = new SwapShopEtsyDetail
            {
                CountryCode = shop.CountryIso ?? string.Empty,
                IsVacationMode = shop.IsVacation,
                ShopCurrency = shop.CurrencyCode ?? string.Empty,
                ShopUrl = shop.Url ?? string.Empty
            },
            Platform = "Etsy",
            PlatformAccountId = platformAccountId,
            PlatformShopId = shop.ShopId.ToString(),
            RefreshToken = tokens.RefreshToken,
            RefreshTokenExpiryDateTime = now.AddDays(EtsyRefreshTokenLifetimeDays),
            ShopConnectionId = shopConnectionId,
            ShopName = shop.ShopName ?? string.Empty,
            TokenExpiryDateTime = now.AddSeconds(tokens.ExpiresIn),
            UserId = userId
        };

        return await ExecuteSwapAsync(context, swapShop, input, EtsyChallengeCookieName, "etsy");
    }

    private static async Task<IResult> SwapGumroadAsync(HttpContext context, SwapShopAccountCommand swapShop, long userId, long shopConnectionId, GumroadTokenResponse tokens, GumroadUser user)
    {
        var platformShopId = string.IsNullOrEmpty(user.UserId) ? user.Id : user.UserId;
        var input = new SwapShopAccountInput
        {
            AccessToken = tokens.AccessToken,
            Platform = "Gumroad",
            PlatformAccountId = platformShopId,
            PlatformShopId = platformShopId,
            RefreshToken = null,
            ShopConnectionId = shopConnectionId,
            ShopName = string.IsNullOrEmpty(user.Name) ? (user.Email ?? "Gumroad") : user.Name,
            UserId = userId
        };

        return await ExecuteSwapAsync(context, swapShop, input, GumroadChallengeCookieName, "gumroad");
    }

    private static async Task<IResult> SwapStripeAsync(HttpContext context, SwapShopAccountCommand swapShop, long userId, long shopConnectionId, StripeAccountInfo account, string accessToken, string? refreshToken)
    {
        var input = new SwapShopAccountInput
        {
            AccessToken = accessToken,
            Platform = "Stripe",
            PlatformAccountId = account.AccountId,
            PlatformShopId = account.AccountId,
            RefreshToken = refreshToken,
            ShopConnectionId = shopConnectionId,
            ShopName = string.IsNullOrEmpty(account.BusinessName) ? (account.Email ?? "Stripe") : account.BusinessName,
            UserId = userId
        };

        return await ExecuteSwapAsync(context, swapShop, input, StripeChallengeCookieName, "stripe");
    }

    private static async Task<IResult> ExecuteSwapAsync(HttpContext context, SwapShopAccountCommand swapShop, SwapShopAccountInput input, string cookieName, string platformQueryKey)
    {
        // TRANSACTION
        var result = await EnforcedTransactionScope.ExecuteAsync(async () => await swapShop.ExecuteAsync(input));
        ClearCookie(context, cookieName);

        if (!result.Found)
            return Results.Redirect($"/platforms?error={platformQueryKey}_reconnect_notfound");

        return Results.Redirect($"/platforms?{platformQueryKey}={(result.Swapped ? "swapped" : "refreshed")}");
    }

    private static StripeAccountInfo ToAccountInfo(Stripe.Account account, string fallbackAccountId)
    {
        // We retrieve the account by stripe_user_id, so account.Id should be populated. The OAuth-provided
        // fallback is defensive only — keeps the AccountInfo valid if a future SDK change ever returns a null Id.
        var accountId = string.IsNullOrEmpty(account.Id) ? fallbackAccountId : account.Id;
        var businessName = account.BusinessProfile?.Name
            ?? account.Settings?.Dashboard?.DisplayName
            ?? account.Email
            ?? "Stripe";

        return new StripeAccountInfo
        {
            AccountId = accountId,
            BusinessName = businessName,
            Email = account.Email
        };
    }

    private static void ClearCookie(HttpContext context, string cookieName)
    {
        context.Response.Cookies.Delete(cookieName);
    }

    private static CookieOptions ToCookieOptions(HttpContext context)
    {
        // 60 min comfortably covers any platform's OAuth approval flow (user might pause on the consent screen,
        // 2FA, switch accounts, etc.) — shared across Etsy, Gumroad, and Stripe.
        return new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddMinutes(60),
            HttpOnly = true,
            IsEssential = true,
            Path = "/",
            SameSite = SameSiteMode.Lax,
            Secure = context.Request.IsHttps
        };
    }

    #endregion
}

/// <summary>Cookie</summary>
internal class EtsyOAuthCookie
{
    #region <Properties>

    public string CodeVerifier { get; set; } = string.Empty;

    /// <summary>Set on a reconnect flow — the shop being re-authorized. Null on a fresh connect / add-shop flow.</summary>
    public long? ReconnectShopConnectionId { get; set; }

    public string State { get; set; } = string.Empty;

    public long UserId { get; set; }

    #endregion
}

/// <summary>Cookie</summary>
internal class GumroadOAuthCookie
{
    #region <Properties>

    /// <summary>Set on a reconnect flow — the shop being re-authorized. Null on a fresh connect / add-shop flow.</summary>
    public long? ReconnectShopConnectionId { get; set; }

    public string State { get; set; } = string.Empty;

    public long UserId { get; set; }

    #endregion
}

/// <summary>Cookie</summary>
internal class StripeOAuthCookie
{
    #region <Properties>

    /// <summary>Set on a reconnect flow — the shop being re-authorized. Null on a fresh connect / add-shop flow.</summary>
    public long? ReconnectShopConnectionId { get; set; }

    public string State { get; set; } = string.Empty;

    public long UserId { get; set; }

    #endregion
}
