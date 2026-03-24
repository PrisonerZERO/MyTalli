namespace My.Talli.Web.Services.Authentication;

using Domain.Components.Tokens;
using Domain.Framework;
using Domain.Handlers.Authentication;
using Domain.Notifications.Emails;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Logging;
using Services.Email;
using System.Security.Claims;

/// <summary>Handler</summary>
public class AppleAuthenticationHandler
{
    #region <Variables>

    private readonly IEmailService _emailService;
    private readonly ILogger<AppleAuthenticationHandler> _logger;
    private readonly AppleSignInHandler _signInHandler;
    private readonly UnsubscribeTokenService _unsubscribeTokenService;


    #endregion

    #region <Constructors>

    public AppleAuthenticationHandler(IEmailService emailService, ILogger<AppleAuthenticationHandler> logger, AppleSignInHandler signInHandler, UnsubscribeTokenService unsubscribeTokenService)
    {
        _emailService = emailService;
        _logger = logger;
        _signInHandler = signInHandler;
        _unsubscribeTokenService = unsubscribeTokenService;
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
            var u = await _signInHandler.HandleAsync(argument);

            // Claims
            var identity = (ClaimsIdentity)principal.Identity!;
            identity.AddClaim(new Claim("UserId", u.Id.ToString()));

            foreach (var role in u.Roles)
                identity.AddClaim(new Claim(ClaimTypes.Role, role));

            return u;
        });

        // Welcome Email
        if (user.IsNewUser)
            await SendWelcomeEmailAsync(argument.Email, user.FirstName, user.Id);
    }

    private async Task SendWelcomeEmailAsync(string email, string firstName, long userId)
    {
        try
        {
            var smtp = new WelcomeEmailNotification().Build(new EmailNotificationArgumentOf<WelcomeEmailNotificationPayload>
            {
                Payload = new WelcomeEmailNotificationPayload { FirstName = firstName, UnsubscribeToken = _unsubscribeTokenService.GenerateToken(userId) }
            });

            smtp.To = [email];
            await _emailService.SendAsync(smtp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email for user {UserId}", userId);
        }
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
