namespace My.Talli.Web.Services.Authentication;

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.OAuth;
using My.Talli.Domain.Handlers.Authentication;

/// <summary>Handler</summary>
public class GoogleAuthenticationHandler
{
    #region <Variables>

    private readonly GoogleSignInHandler _signInHandler;

    #endregion

    #region <Constructors>

    public GoogleAuthenticationHandler(GoogleSignInHandler signInHandler)
    {
        _signInHandler = signInHandler;
    }

    #endregion

    #region <Methods>

    public async Task HandleTicketAsync(OAuthCreatingTicketContext context)
    {
        var principal = context.Principal!;
        var avatarUrl = principal.FindFirstValue("urn:google:picture") ?? string.Empty;
        var displayName = principal.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
        var email = principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var emailVerified = principal.FindFirstValue("urn:google:email_verified") == "true";
        var firstName = principal.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty;
        var googleId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var lastName = principal.FindFirstValue(ClaimTypes.Surname) ?? string.Empty;
        var locale = principal.FindFirstValue("urn:google:locale") ?? string.Empty;

        // Sign-In
        var user = await _signInHandler.HandleAsync(googleId, email, displayName, firstName, lastName, avatarUrl, emailVerified, locale);

        var identity = (ClaimsIdentity)principal.Identity!;
        identity.AddClaim(new Claim("UserId", user.Id.ToString()));
    }

    #endregion
}
