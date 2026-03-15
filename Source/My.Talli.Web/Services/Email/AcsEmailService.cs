namespace My.Talli.Web.Services.Email;

using Azure;
using Azure.Communication.Email;
using Domain.Notifications.Emails;
using Microsoft.Extensions.Options;

/// <summary>Service</summary>
public class AcsEmailService : IEmailService
{
    #region <Variables>

    private readonly EmailClient _client;
    private readonly EmailSettings _settings;
    private readonly ILogger<AcsEmailService> _logger;

    #endregion

    #region <Constructors>

    public AcsEmailService(IConfiguration configuration, ILogger<AcsEmailService> logger, IOptions<EmailSettings> settings)
    {
        _client = new EmailClient(configuration["AzureCommunicationServices:ConnectionString"]);
        _logger = logger;
        _settings = settings.Value;
    }

    #endregion

    #region <Methods>

    public async Task SendAsync(SmtpNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            var from = string.IsNullOrEmpty(notification.From)
                ? $"{_settings.FromDisplayName} <{_settings.FromAddress}>"
                : notification.From;

            var content = notification.IsBodyHtml
                ? new EmailContent(notification.Subject) { Html = notification.Body }
                : new EmailContent(notification.Subject) { PlainText = notification.Body };

            var recipients = new EmailRecipients(
                notification.To.Select(to => new EmailAddress(to)).ToList());

            if (notification.Cc.Count > 0)
            {
                foreach (var cc in notification.Cc)
                    recipients.CC.Add(new EmailAddress(cc));
            }

            if (notification.Bcc.Count > 0)
            {
                foreach (var bcc in notification.Bcc)
                    recipients.BCC.Add(new EmailAddress(bcc));
            }

            var message = new EmailMessage(from, recipients, content);

            var operation = await _client.SendAsync(WaitUntil.Started, message, cancellationToken);

            _logger.LogInformation("Email queued via ACS. Subject: {Subject}, To: {To}, OperationId: {OperationId}",
                notification.Subject, string.Join(", ", notification.To), operation.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via ACS. Subject: {Subject}, To: {To}",
                notification.Subject, string.Join(", ", notification.To));
        }
    }

    #endregion
}
