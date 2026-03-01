using Microsoft.AspNetCore.Authentication.OAuth;
using System.Security.Claims;

namespace My.Talli.Web.Services.Authentication;

public class AppleAuthenticationHandler
{
    #region <Variables>

    private readonly ILogger<AppleAuthenticationHandler> _logger;

    #endregion

    #region <Constructors>

    public AppleAuthenticationHandler(ILogger<AppleAuthenticationHandler> logger)
    {
        _logger = logger;
    }

    #endregion

    #region <Methods>

    public async Task HandleTicketAsync(OAuthCreatingTicketContext context)
    {
        var email = context.Principal?.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var appleId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var name = context.Principal?.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

        // Apple only returns the user's name on the FIRST sign-in.
        // Subsequent logins only include email and subject (NameIdentifier).
        // When a user record is created, the name must be persisted in the database.
        _logger.LogInformation("Apple sign-in for {Email} ({AppleId})", email, appleId);

        // TODO: Look up user in database by Apple ID or email
        // TODO: If user does not exist, create a new user record (and store name from first sign-in)
        // TODO: If user exists, update last login timestamp
        // TODO: Add app-specific claims (e.g., internal user ID, roles) to the identity:
        //   var identity = (ClaimsIdentity)context.Principal!.Identity!;
        //   identity.AddClaim(new Claim("UserId", dbUser.Id.ToString()));

        await Task.CompletedTask;
    }

    #endregion
}
