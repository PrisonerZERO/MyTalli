namespace My.Talli.Web.Services.Authentication;

using Domain.Components.Tokens;
using Domain.Handlers.Authentication;
using Domain.Notifications.Emails;
using Microsoft.AspNetCore.Authentication.OAuth;
using Services.Email;
using System.Security.Claims;

/// <summary>Handler</summary>
public class GoogleAuthenticationHandler
{
    #region <Variables>

    private readonly IEmailService _emailService;
    private readonly GoogleSignInHandler _signInHandler;
    private readonly UnsubscribeTokenService _unsubscribeTokenService;


    #endregion

    #region <Constructors>

    public GoogleAuthenticationHandler(
        IEmailService emailService,
        GoogleSignInHandler signInHandler,
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

        if (user.IsNewUser)
            await SendWelcomeEmailAsync(argument.Email, user.FirstName, user.Id);

        var identity = (ClaimsIdentity)principal.Identity!;
        identity.AddClaim(new Claim("UserId", user.Id.ToString()));

        foreach (var role in user.Roles)
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
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
