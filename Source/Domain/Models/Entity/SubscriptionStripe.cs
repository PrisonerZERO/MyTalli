namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class SubscriptionStripe : DefaultModel
{
	#region <Properties>

	public string StripeCustomerId { get; set; } = string.Empty;

	public string StripePriceId { get; set; } = string.Empty;

	public string StripeSubscriptionId { get; set; } = string.Empty;

	#endregion
}
