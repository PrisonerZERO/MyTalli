namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class EmailPreferences
{
	#region <Properties>

	public bool SubscriptionConfirmationEmail { get; set; } = true;

	public bool UnsubscribeAll { get; set; }

	public bool WeeklySummaryEmail { get; set; } = true;


	#endregion
}
