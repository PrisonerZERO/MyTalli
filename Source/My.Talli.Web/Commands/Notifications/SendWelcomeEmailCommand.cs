namespace My.Talli.Web.Commands.Notifications;

using Domain.Components.Tokens;
using Domain.Notifications.Emails;
using Web.Services.Email;

/// <summary>Command</summary>
public class SendWelcomeEmailCommand
{
    #region <Variables>

    private readonly IEmailService _emailService;
    private readonly ILogger<SendWelcomeEmailCommand> _logger;
    private readonly UnsubscribeTokenService _tokenService;

    #endregion

    #region <Constructors>

    public SendWelcomeEmailCommand(IEmailService emailService, ILogger<SendWelcomeEmailCommand> logger, UnsubscribeTokenService tokenService)
    {
        _emailService = emailService;
        _logger = logger;
        _tokenService = tokenService;
    }

    #endregion

    #region <Methods>

    public async Task ExecuteAsync(string email, string firstName, long userId)
    {
        try
        {
            var notification = new WelcomeEmailNotification();
            var unsubscribeToken = _tokenService.GenerateToken(userId);
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

    #endregion
}
