namespace My.Talli.Domain.Notifications.Emails;

/// <summary>Notification Argument</summary>
public class EmailNotificationArgumentOf<T> : EmailNotificationArgument
{
    #region <Properties>

    public required T Payload { get; init; }


    #endregion
}
