namespace My.Talli.Web.Services.Authentication;

using Domain.Handlers.Authentication;
using Domain.Notifications.Emails;
using Microsoft.AspNetCore.Authentication.OAuth;
using Services.Email;
using System.Security.Claims;

/// <summary>Handler</summary>
public class AppleAuthenticationHandler
{
    #region <Variables>

    private readonly IEmailService _emailService;
    private readonly AppleSignInHandler _signInHandler;

    #endregion

    #region <Constructors>

    public AppleAuthenticationHandler(
        IEmailService emailService,
        AppleSignInHandler signInHandler)
    {
        _emailService = emailService;
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