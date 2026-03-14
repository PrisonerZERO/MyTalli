namespace My.Talli.Web.Services.Authentication;

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.OAuth;
using My.Talli.Domain.Handlers.Authentication;

/// <summary>Handler</summary>
public class AppleAuthenticationHandler
{
    #region <Variables>

    private readonly AppleSignInHandler _signInHandler;

    #endregion

    #region <Constructors>

    public AppleAuthenticationHandler(AppleSignInHandler signInHandler)
    {
        _signInHandler = signInHandler;
    }

    #endregion

    #region <Methods>

    public async Task HandleTicketAsync(OAuthCreatingTicketContext context)
    {
        var principal = context.Principal!;
        var appleId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var displayName = principal.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
        var email = principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var firstName = principal.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty;
        var isPrivateRelay = principal.FindFirstValue("urn:apple:is_private_email") == "true";
        var lastName = principal.FindFirstValue(ClaimTypes.Surname) ?? string.Empty;

        var user = await _signInHandler.HandleAsync(appleId, email, displayName, firstName, lastName, isPrivateRelay);

        var identity = (ClaimsIdentity)principal.Identity!;
        identity.AddClaim(new Claim("UserId", user.Id.ToString()));
    }

    #endregion
}
