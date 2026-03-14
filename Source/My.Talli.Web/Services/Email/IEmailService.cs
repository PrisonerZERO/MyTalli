using My.Talli.Domain.Notifications.Emails;

namespace My.Talli.Web.Services.Email;

public interface IEmailService
{
    #region <Methods>

    Task SendAsync(SmtpNotification notification, CancellationToken cancellationToken = default);

    #endregion
}
