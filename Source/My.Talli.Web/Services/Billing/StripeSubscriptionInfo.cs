namespace My.Talli.Web.Services.Billing;

/// <summary>DTO</summary>
public class StripeSubscriptionInfo
{
	#region <Properties>

	public required bool CancelAtPeriodEnd { get; init; }

	public required DateTime CurrentPeriodEnd { get; init; }

	public required string Status { get; init; }

	public required string SubscriptionId { get; init; }

	#endregion
}
