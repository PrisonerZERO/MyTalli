namespace My.Talli.Domain.Notifications.Emails;

/// <summary>Payload</summary>
public class WaitlistWelcomeEmailNotificationPayload
{
	#region <Properties>

	public string FirstName { get; set; } = string.Empty;

	public string UnsubscribeToken { get; set; } = string.Empty;

	#endregion
}
