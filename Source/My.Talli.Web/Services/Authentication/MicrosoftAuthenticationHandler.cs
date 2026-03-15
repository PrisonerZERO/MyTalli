namespace My.Talli.Web.Services.Authentication;

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

    #endregion

    #region <Constructors>

    public MicrosoftAuthenticationHandler(
        IEmailService emailService,
        MicrosoftSignInHandler signInHandler)
    {
        _emailService = emailService;
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

        if (user.IsNewUser)
            await SendWaitlistWelcomeEmailAsync(argument.Email, user.FirstName);

        var identity = (ClaimsIdentity)principal.Identity!;
        identity.AddClaim(new Claim("UserId", user.Id.ToString()));

        foreach (var role in user.Roles)
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
    }

    private async Task SendWaitlistWelcomeEmailAsync(string email, string firstName)
    {
        var notification = new WaitlistWelcomeEmailNotification();
        var smtp = notification.Build(new EmailNotificationArgumentOf<WaitlistWelcomeEmailNotificationPayload>
        {
            Payload = new WaitlistWelcomeEmailNotificationPayload { FirstName = firstName }
        });
        smtp.To = [email];
        await _emailService.SendAsync(smtp);
    }

    #endregion
}