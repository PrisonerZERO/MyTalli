namespace My.Talli.Domain.Notifications.Emails;

/// <summary>Notification</summary>
public abstract class EmailNotificationOf<T> : EmailNotification
{
    #region <Methods>

    public abstract SmtpNotification Build(EmailNotificationArgumentOf<T> arguments);

    #endregion
}
