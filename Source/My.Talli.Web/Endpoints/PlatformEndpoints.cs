namespace My.Talli.Web.Endpoints;

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

    #endregion

    #region <Endpoints>

    public static void MapPlatformEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/platforms/etsy/connect", EtsyConnect).RequireAuthorization();
        app.MapGet("/api/platforms/etsy/callback", EtsyCallback).RequireAuthorization();
    }

    #endregion

    #region <Methods>

    private static async Task<IResult> EtsyConnect(
        HttpContext context,
        EtsyService etsy,
        IDataProtectionProvider dataProtectionProvider,
        RepositoryAdapterAsync<MODELS.ShopConnection, ENTITIES.ShopConnection> shopConnectionAdapter,
        RepositoryAdapterAsync<MODELS.Subscription, ENTITIES.Subscription> subscriptionAdapter)
    {
        var userIdClaim = context.User.FindFirst("UserId")?.Value;
        if (!long.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();

        // Plan-tier guard: free tier is capped at 1 Etsy shop. Pro (ProductId 1 or 2, Active/Cancelling) is uncapped.
        var isPro = (await subscriptionAdapter.FindAsync(s =>
            s.UserId == userId &&
            (s.ProductId == 1 || s.ProductId == 2) &&
            (s.Status == SubscriptionStatuses.Active || s.Status == SubscriptionStatuses.Cancelling))).Any();

        if (!isPro)
        {
            var etsyShopCount = (await shopConnectionAdapter.FindAsync(s =>
                s.UserId == userId && s.PlatformConnection.Platform == "Etsy")).Count();

            if (etsyShopCount >= 1)
                return Results.Redirect("/platforms?error=plan_limit");
        }

        var challenge = etsy.BuildAuthorizeChallenge();
        var protector = dataProtectionProvider.CreateProtector(EtsyChallengePurpose);
        var cookiePayload = JsonSerializer.Serialize(new EtsyOAuthCookie { CodeVerifier = challenge.CodeVerifier, State = challenge.State, UserId = userId });
        var protectedPayload = protector.Protect(cookiePayload);

        context.Response.Cookies.Append(EtsyChallengeCookieName, protectedPayload, ToCookieOptions(context));

        return Results.Redirect(challenge.AuthorizeUrl);
    }

    private static async Task<IResult> EtsyCallback(HttpContext context, EtsyService etsy, IDataProtectionProvider dataProtectionProvider, ConnectEtsyCommand connectEtsy, ILogger<Program> logger)
    {
        var error = context.Request.Query["error"].ToString();
        if (!string.IsNullOrEmpty(error))
        {
            logger.LogWarning("Etsy OAuth returned error: {Error}", error);
            ClearChallengeCookie(context);
            return Results.Redirect("/platforms?error=etsy_denied");
        }

        var code = context.Request.Query["code"].ToString();
        var state = context.Request.Query["state"].ToString();
        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
        {
            ClearChallengeCookie(context);
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
            ClearChallengeCookie(context);
            return Results.Redirect("/platforms?error=etsy_expired");
        }

        if (cookie is null || cookie.State != state)
        {
            logger.LogWarning("Etsy OAuth state mismatch for user {UserId}", cookie?.UserId);
            ClearChallengeCookie(context);
            return Results.Redirect("/platforms?error=etsy_state");
        }

        try
        {
            var tokenResponse = await etsy.ExchangeCodeAsync(code, cookie.CodeVerifier);
            var platformAccountId = EtsyPkceGenerator.ExtractEtsyUserId(tokenResponse.AccessToken);
            var shops = await etsy.GetShopsAsync(platformAccountId, tokenResponse.AccessToken);

            // TRANSACTION
            var result = await EnforcedTransactionScope.ExecuteAsync(async () => await connectEtsy.ExecuteAsync(cookie.UserId, tokenResponse, platformAccountId, shops));

            logger.LogInformation("Etsy OAuth connected for user {UserId}: firstConnection={IsFirst} new={NewShopCount} refreshed={RefreshedShopCount}", cookie.UserId, result.IsFirstConnection, result.NewShopCount, result.RefreshedShopCount);
            ClearChallengeCookie(context);

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
            ClearChallengeCookie(context);
            return Results.Redirect("/platforms?error=etsy_exchange");
        }
    }

    private static void ClearChallengeCookie(HttpContext context)
    {
        context.Response.Cookies.Delete(EtsyChallengeCookieName);
    }

    private static CookieOptions ToCookieOptions(HttpContext context)
    {
        return new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddMinutes(10),
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

    public string State { get; set; } = string.Empty;

    public long UserId { get; set; }

    #endregion
}
