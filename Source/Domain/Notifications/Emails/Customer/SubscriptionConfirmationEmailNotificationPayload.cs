namespace My.Talli.Domain.Notifications.Emails;

public class SubscriptionConfirmationEmailNotificationPayload
{
    #region <Properties>

    public string Amount { get; set; } = string.Empty;

    public string CardLastFour { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string Plan { get; set; } = string.Empty;

    public string RenewalDate { get; set; } = string.Empty;

    #endregion
}
