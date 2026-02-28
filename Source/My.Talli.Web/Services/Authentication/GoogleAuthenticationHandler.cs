using Microsoft.AspNetCore.Authentication.OAuth;
using System.Security.Claims;

namespace My.Talli.Web.Services.Authentication;

public class GoogleAuthenticationHandler
{
    #region <Variables>

    private readonly ILogger<GoogleAuthenticationHandler> _logger;

    #endregion

    #region <Constructors>

    public GoogleAuthenticationHandler(ILogger<GoogleAuthenticationHandler> logger)
    {
        _logger = logger;
    }

    #endregion

    #region <Methods>

    public async Task HandleTicketAsync(OAuthCreatingTicketContext context)
    {
        var email = context.Principal?.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var googleId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var name = context.Principal?.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
        var pictureUrl = context.Principal?.FindFirstValue("urn:google:picture") ?? string.Empty;

        _logger.LogInformation("Google sign-in for {Email} ({GoogleId})", email, googleId);

        // TODO: Look up user in database by Google ID or email
        // TODO: If user does not exist, create a new user record
        // TODO: If user exists, update last login timestamp
        // TODO: Add app-specific claims (e.g., internal user ID, roles) to the identity:
        //   var identity = (ClaimsIdentity)context.Principal!.Identity!;
        //   identity.AddClaim(new Claim("UserId", dbUser.Id.ToString()));

        await Task.CompletedTask;
    }

    #endregion
}
