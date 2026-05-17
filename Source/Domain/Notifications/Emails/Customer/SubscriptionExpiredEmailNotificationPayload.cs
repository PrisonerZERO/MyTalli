namespace My.Talli.Domain.Notifications.Emails;

/// <summary>Payload</summary>
public class SubscriptionExpiredEmailNotificationPayload
{
	#region <Properties>

	public string EndDateDisplay { get; set; } = string.Empty;

	public string FirstName { get; set; } = string.Empty;

	public string PlanName { get; set; } = string.Empty;

	public string UnsubscribeToken { get; set; } = string.Empty;

	#endregion
}
