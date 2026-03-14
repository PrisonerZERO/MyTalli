namespace My.Talli.Domain.Notifications.Emails;

public class EmailNotificationArgumentOf<T> : EmailNotificationArgument
{
    #region <Properties>

    public required T Payload { get; init; }

    #endregion
}
