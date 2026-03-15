namespace My.Talli.Web.Services.Authentication;

using Domain.Handlers.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Security.Claims;

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

        var argument = new SignInArgumentOf<AppleSignInPayload>
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

        var user = await _signInHandler.HandleAsync(argument);

        var identity = (ClaimsIdentity)principal.Identity!;
        identity.AddClaim(new Claim("UserId", user.Id.ToString()));

        foreach (var role in user.Roles)
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
    }

    #endregion
}