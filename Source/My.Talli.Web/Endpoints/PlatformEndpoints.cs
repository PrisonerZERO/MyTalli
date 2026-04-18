namespace My.Talli.Web.Endpoints;

using Domain.Framework;
using Microsoft.AspNetCore.DataProtection;
using System.Text.Json;
using Web.Commands.Endpoints;
using Web.Services.Platforms;

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

    private static IResult EtsyConnect(HttpContext context, EtsyService etsy, IDataProtectionProvider dataProtectionProvider)
    {
        var userIdClaim = context.User.FindFirst("UserId")?.Value;
        if (!long.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();

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
            var platformAccountId = EtsyService.ExtractEtsyUserId(tokenResponse.AccessToken);
            var shops = await etsy.GetShopsAsync(platformAccountId, tokenResponse.AccessToken);

            // TRANSACTION
            await EnforcedTransactionScope.ExecuteAsync(async () => await connectEtsy.ExecuteAsync(cookie.UserId, tokenResponse, platformAccountId, shops));

            logger.LogInformation("Etsy OAuth connected for user {UserId} with {ShopCount} shop(s)", cookie.UserId, shops.Count);
            ClearChallengeCookie(context);
            return Results.Redirect("/platforms?connected=etsy");
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
