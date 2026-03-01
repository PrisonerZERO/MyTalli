using Microsoft.AspNetCore.Authentication.OAuth;
using System.Security.Claims;

namespace My.Talli.Web.Services.Authentication;

public class MicrosoftAuthenticationHandler
{
    #region <Variables>

    private readonly ILogger<MicrosoftAuthenticationHandler> _logger;

    #endregion

    #region <Constructors>

    public MicrosoftAuthenticationHandler(ILogger<MicrosoftAuthenticationHandler> logger)
    {
        _logger = logger;
    }

    #endregion

    #region <Methods>

    public async Task HandleTicketAsync(OAuthCreatingTicketContext context)
    {
        var email = context.Principal?.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var microsoftId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var name = context.Principal?.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

        _logger.LogInformation("Microsoft sign-in for {Email} ({MicrosoftId})", email, microsoftId);

        // TODO: Look up user in database by Microsoft ID or email
        // TODO: If user does not exist, create a new user record
        // TODO: If user exists, update last login timestamp
        // TODO: Add app-specific claims (e.g., internal user ID, roles) to the identity:
        //   var identity = (ClaimsIdentity)context.Principal!.Identity!;
        //   identity.AddClaim(new Claim("UserId", dbUser.Id.ToString()));

        await Task.CompletedTask;
    }

    #endregion
}
