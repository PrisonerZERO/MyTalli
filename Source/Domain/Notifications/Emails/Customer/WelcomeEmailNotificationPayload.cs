namespace My.Talli.Domain.Notifications.Emails;

/// <summary>Payload</summary>
public class WelcomeEmailNotificationPayload
{
    #region <Properties>

    public string FirstName { get; set; } = string.Empty;

    public string UnsubscribeToken { get; set; } = string.Empty;


    #endregion
}
