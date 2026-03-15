namespace My.Talli.Web.Services.Authentication;

using Domain.Handlers.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Security.Claims;

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

        var argument = new SignInArgumentOf<GoogleSignInPayload>
        {
            DisplayName = principal.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
            Email = principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            FirstName = principal.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty,
            LastName = principal.FindFirstValue(ClaimTypes.Surname) ?? string.Empty,
            Payload = new GoogleSignInPayload
            {
                AvatarUrl = principal.FindFirstValue("urn:google:picture") ?? string.Empty,
                EmailVerified = principal.FindFirstValue("urn:google:email_verified") == "true",
                GoogleId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
                Locale = principal.FindFirstValue("urn:google:locale") ?? string.Empty
            }
        };

        var user = await _signInHandler.HandleAsync(argument);

        var identity = (ClaimsIdentity)principal.Identity!;
        identity.AddClaim(new Claim("UserId", user.Id.ToString()));
    }

    #endregion
}