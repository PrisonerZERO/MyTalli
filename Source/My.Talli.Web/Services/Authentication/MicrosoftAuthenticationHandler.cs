namespace My.Talli.Web.Services.Authentication;

using Domain.Components.Tokens;
using Domain.Framework;
using Domain.Handlers.Authentication;
using Domain.Notifications.Emails;
using Microsoft.AspNetCore.Authentication.OAuth;
using Services.Email;
using System.Security.Claims;

/// <summary>Handler</summary>
public class MicrosoftAuthenticationHandler
{
    #region <Variables>

    private readonly IEmailService _emailService;
    private readonly MicrosoftSignInHandler _signInHandler;
    private readonly UnsubscribeTokenService _unsubscribeTokenService;


    #endregion

    #region <Constructors>

    public MicrosoftAuthenticationHandler(
        IEmailService emailService,
        MicrosoftSignInHandler signInHandler,
        UnsubscribeTokenService unsubscribeTokenService)
    {
        _emailService = emailService;
        _signInHandler = signInHandler;
        _unsubscribeTokenService = unsubscribeTokenService;
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

        // TRANSACTION
        var user = await EnforcedTransactionScope.ExecuteAsync(async () =>
        {
            var u = await _signInHandler.HandleAsync(argument);

            var identity = (ClaimsIdentity)principal.Identity!;
            identity.AddClaim(new Claim("UserId", u.Id.ToString()));

            foreach (var role in u.Roles)
                identity.AddClaim(new Claim(ClaimTypes.Role, role));

            return u;
        });

        if (user.IsNewUser)
            await SendWelcomeEmailAsync(argument.Email, user.FirstName, user.Id);
    }

    private async Task SendWelcomeEmailAsync(string email, string firstName, long userId)
    {
        var notification = new WelcomeEmailNotification();
        var smtp = notification.Build(new EmailNotificationArgumentOf<WelcomeEmailNotificationPayload>
        {
            Payload = new WelcomeEmailNotificationPayload
            {
                FirstName = firstName,
                UnsubscribeToken = _unsubscribeTokenService.GenerateToken(userId)
            }
        });
        smtp.To = [email];
        await _emailService.SendAsync(smtp);
    }


    #endregion
}
