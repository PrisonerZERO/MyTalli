namespace My.Talli.Web.Services.Email;

using My.Talli.Domain.Notifications.Emails;

public interface IEmailService
{
    #region <Methods>

    Task SendAsync(SmtpNotification notification, CancellationToken cancellationToken = default);

    #endregion
}
