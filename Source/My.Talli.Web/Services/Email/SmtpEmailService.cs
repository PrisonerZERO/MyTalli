namespace My.Talli.Web.Services.Email;

using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Domain.Notifications.Emails;

/// <summary>Service</summary>
public class SmtpEmailService : IEmailService
{
    #region <Variables>

    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    #endregion

    #region <Constructors>

    public SmtpEmailService(ILogger<SmtpEmailService> logger, IOptions<EmailSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
    }

    #endregion

    #region <Methods>

    public async Task SendAsync(SmtpNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(
                _settings.FromDisplayName,
                string.IsNullOrEmpty(notification.From) ? _settings.FromAddress : notification.From));

            foreach (var to in notification.To)
                message.To.Add(MailboxAddress.Parse(to));

            foreach (var cc in notification.Cc)
                message.Cc.Add(MailboxAddress.Parse(cc));

            foreach (var bcc in notification.Bcc)
                message.Bcc.Add(MailboxAddress.Parse(bcc));

            message.Subject = notification.Subject;

            message.Body = notification.IsBodyHtml
                ? new TextPart("html") { Text = notification.Body }
                : new TextPart("plain") { Text = notification.Body };

            using var client = new SmtpClient();

            var secureSocketOptions = _settings.UseSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

            await client.ConnectAsync(_settings.Host, _settings.Port, secureSocketOptions, cancellationToken);

            if (!string.IsNullOrEmpty(_settings.Username))
                await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Email sent successfully. Subject: {Subject}, To: {To}", notification.Subject, string.Join(", ", notification.To));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email. Subject: {Subject}, To: {To}", notification.Subject, string.Join(", ", notification.To));
        }
    }

    #endregion
}
