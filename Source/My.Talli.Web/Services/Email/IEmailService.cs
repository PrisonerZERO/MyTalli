namespace My.Talli.Web.Services.Email;

using Domain.Notifications.Emails;

/// <summary>Service</summary>
public interface IEmailService
{
    #region <Methods>

    Task SendAsync(SmtpNotification notification, CancellationToken cancellationToken = default);

    #endregion
}
