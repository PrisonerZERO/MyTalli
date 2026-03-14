namespace My.Talli.Web.Services.Authentication;

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.OAuth;
using My.Talli.Domain.Handlers.Authentication;

/// <summary>Handler</summary>
public class MicrosoftAuthenticationHandler
{
    #region <Variables>

    private readonly MicrosoftSignInHandler _signInHandler;

    #endregion

    #region <Constructors>

    public MicrosoftAuthenticationHandler(MicrosoftSignInHandler signInHandler)
    {
        _signInHandler = signInHandler;
    }

    #endregion

    #region <Methods>

    public async Task HandleTicketAsync(OAuthCreatingTicketContext context)
    {
        var principal = context.Principal!;

        var argument = new SignInArgumentOf<MicrosoftSignInPayload>
        {
            DisplayName = principal.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
            Email = principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            FirstName = principal.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty,
            LastName = principal.FindFirstValue(ClaimTypes.Surname) ?? string.Empty,
            Payload = new MicrosoftSignInPayload
            {
                MicrosoftId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty
            }
        };

        var user = await _signInHandler.HandleAsync(argument);

        var identity = (ClaimsIdentity)principal.Identity!;
        identity.AddClaim(new Claim("UserId", user.Id.ToString()));
    }

    #endregion
}
