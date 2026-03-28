namespace My.Talli.Domain.Handlers.Billing;

/// <summary>Payload</summary>
public class SubscriptionUpdatedPayload
{
	#region <Properties>

	public bool CancelAtPeriodEnd { get; set; }

	public DateTime CurrentPeriodEnd { get; set; }

	public string Status { get; set; } = string.Empty;

	public long? ProductId { get; set; }

	public string StripePriceId { get; set; } = string.Empty;

	public string StripeSubscriptionId { get; set; } = string.Empty;


	#endregion
}
