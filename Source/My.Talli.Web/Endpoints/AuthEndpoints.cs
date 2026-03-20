namespace My.Talli.Web.Endpoints;

using AspNet.Security.OAuth.Apple;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;

/// <summary>Endpoint</summary>
public static class AuthEndpoints
{
    #region <Methods>
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/auth/login/{provider}", async (string provider, HttpContext context) =>
        {
            var properties = new AuthenticationProperties { RedirectUri = "/waitlist" };
            var scheme = provider.ToLowerInvariant() switch
            {
                "apple" => AppleAuthenticationDefaults.AuthenticationScheme,
                "google" => GoogleDefaults.AuthenticationScheme,
                "microsoft" => MicrosoftAccountDefaults.AuthenticationScheme,
                _ => throw new ArgumentException($"Unsupported provider: {provider}")
            };

            await context.ChallengeAsync(scheme, properties);
        });

        app.MapGet("/api/auth/logout", async (HttpContext context) =>
        {
            var name = context.User.Identity?.Name;
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var redirect = string.IsNullOrEmpty(name)
                ? "/?signed-out"
                : $"/?signed-out&name={Uri.EscapeDataString(name)}";

            context.Response.Redirect(redirect);
        });
    }
    #endregion
}
