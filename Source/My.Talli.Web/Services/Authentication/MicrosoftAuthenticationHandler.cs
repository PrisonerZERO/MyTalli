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
public class MicrosoftAuthenticationHandler
{
    #region <Variables>

    private readonly IEmailService _emailService;
    private readonly ILogger<MicrosoftAuthenticationHandler> _logger;
    private readonly MicrosoftSignInHandler _signInHandler;
    private readonly UnsubscribeTokenService _unsubscribeTokenService;


    #endregion

    #region <Constructors>

    public MicrosoftAuthenticationHandler(
        IEmailService emailService,
        ILogger<MicrosoftAuthenticationHandler> logger,
        MicrosoftSignInHandler signInHandler,
        UnsubscribeTokenService unsubscribeTokenService)
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
            // Sign-In
            var user = await _signInHandler.HandleAsync(argument);

            // Add Claims
            var identity = (ClaimsIdentity)principal.Identity!;
            identity.AddClaim(new Claim("UserId", user.Id.ToString()));

            foreach (var role in user.Roles)
                identity.AddClaim(new Claim(ClaimTypes.Role, role));

            return user;
        });

        // Email
        if (user.IsNewUser)
            await SendWelcomeEmailAsync(argument.Email, user.FirstName, user.Id);
    }

    private async Task SendWelcomeEmailAsync(string email, string firstName, long userId)
    {
        try
        {
            var notification = new WelcomeEmailNotification();
            var unsubscribeToken = _unsubscribeTokenService.GenerateToken(userId);
            var notificationPayload = new WelcomeEmailNotificationPayload { FirstName = firstName, UnsubscribeToken = unsubscribeToken };
            var notificationArgument = new EmailNotificationArgumentOf<WelcomeEmailNotificationPayload> { Payload = notificationPayload };
            var smtp = notification.Build(notificationArgument);

            smtp.To = [email];
            await _emailService.SendAsync(smtp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email for user {UserId}", userId);
        }
    }

    private static SignInArgumentOf<MicrosoftSignInPayload> ToSignInArgument(ClaimsPrincipal principal)
    {
        return new SignInArgumentOf<MicrosoftSignInPayload>
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
    }


    #endregion
}
