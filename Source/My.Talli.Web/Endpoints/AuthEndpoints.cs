namespace My.Talli.Web.Endpoints;

using AspNet.Security.OAuth.Apple;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;

/// <summary>Endpoint</summary>
public static class AuthEndpoints
{
    #region <Endpoints>

    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/auth/login/{provider}", Login);
        app.MapGet("/api/auth/logout", Logout);
    }

    #endregion

    #region <Methods>

    private static async Task Login(string provider, HttpContext context)
    {
        var properties = new AuthenticationProperties { RedirectUri = "/dashboard" };
        var scheme = provider.ToLowerInvariant() switch
        {
            "apple" => AppleAuthenticationDefaults.AuthenticationScheme,
            "google" => GoogleDefaults.AuthenticationScheme,
            "microsoft" => MicrosoftAccountDefaults.AuthenticationScheme,
            _ => throw new ArgumentException($"Unsupported provider: {provider}")
        };

        await context.ChallengeAsync(scheme, properties);
    }

    private static async Task Logout(HttpContext context)
    {
        var name = context.User.Identity?.Name;
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var redirect = string.IsNullOrEmpty(name)
            ? "/?signed-out"
            : $"/?signed-out&name={Uri.EscapeDataString(name)}";

        context.Response.Redirect(redirect);
    }

    #endregion
}
