namespace My.Talli.Domain.Entities.Entities;

/// <summary>Entity</summary>
public class SubscriptionStripe : DefaultEntity
{
	#region <Properties>

	public string StripeCustomerId { get; set; } = string.Empty;

	public string StripePriceId { get; set; } = string.Empty;

	public string StripeSubscriptionId { get; set; } = string.Empty;

	public Subscription Subscription { get; set; } = null!;

	public long SubscriptionId { get; set; }

	#endregion
}
