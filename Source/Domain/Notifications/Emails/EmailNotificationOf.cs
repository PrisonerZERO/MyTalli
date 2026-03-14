namespace My.Talli.Domain.Notifications.Emails;

public abstract class EmailNotificationOf<T> : EmailNotification
{
    #region <Methods>

    public abstract SmtpNotification Build(EmailNotificationArgumentOf<T> arguments);

    #endregion
}
