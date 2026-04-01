namespace My.Talli.Web.Handlers.Authentication;

using Domain.Components.JsonSerializers;
using Domain.Framework;
using Domain.Handlers.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Security.Claims;
using Web.Commands.Notifications;

/// <summary>Handler</summary>
public class AppleAuthenticationHandler
{
    #region <Variables>

    private readonly AppleSignInHandler _signInHandler;
    private readonly SendWelcomeEmailCommand _sendWelcomeEmail;
    private readonly UserPreferencesJsonSerializer _preferencesSerializer;

    #endregion

    #region <Constructors>

    public AppleAuthenticationHandler(
        AppleSignInHandler signInHandler,
        SendWelcomeEmailCommand sendWelcomeEmail,
        UserPreferencesJsonSerializer preferencesSerializer)
    {
        _preferencesSerializer = preferencesSerializer;
        _sendWelcomeEmail = sendWelcomeEmail;
        _signInHandler = signInHandler;
    }

    #endregion

    #region <Methods>

    public async Task HandleTicketAsync(OAuthCreatingTicketContext context)
    {
        var principal = context.Principal!;
        var argument = ToSignInArgument(principal);

        // TRANSACTION
        var user = await EnforcedTransactionScope.ExecuteAsync(async () =>
        {
            // Sign-In
            var user = await _signInHandler.HandleAsync(argument);

            // Add Claims
            var identity = (ClaimsIdentity)principal.Identity!;
            identity.AddClaim(new Claim("UserId", user.Id.ToString()));

            foreach (var role in user.Roles)
                identity.AddClaim(new Claim(ClaimTypes.Role, role));

            return user;
        });

        // Theme cookie — so theme.js can apply the correct theme before Blazor starts
        var preferences = _preferencesSerializer.Deserialize(user.UserPreferences);
        context.HttpContext.Response.Cookies.Append("talli-theme", preferences.DarkMode ?? "system", new CookieOptions { Path = "/", HttpOnly = false, SameSite = SameSiteMode.Lax, Expires = DateTimeOffset.UtcNow.AddDays(30) });

        // Email
        if (user.IsNewUser)
            await _sendWelcomeEmail.ExecuteAsync(argument.Email, user.FirstName, user.Id);
    }

    private static SignInArgumentOf<AppleSignInPayload> ToSignInArgument(ClaimsPrincipal principal)
    {
        return new SignInArgumentOf<AppleSignInPayload>
        {
            DisplayName = principal.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
            Email = principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            FirstName = principal.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty,
            LastName = principal.FindFirstValue(ClaimTypes.Surname) ?? string.Empty,
            Payload = new AppleSignInPayload
            {
                AppleId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
                IsPrivateRelay = principal.FindFirstValue("urn:apple:is_private_email") == "true"
            }
        };
    }

    #endregion
}
