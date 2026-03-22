namespace My.Talli.Domain.Handlers.Billing;

/// <summary>Payload</summary>
public class CheckoutCompletedPayload
{
	#region <Properties>

	public string CardBrand { get; set; } = string.Empty;

	public string CardLastFour { get; set; } = string.Empty;

	public DateTime CurrentPeriodEnd { get; set; }

	public string CustomerEmail { get; set; } = string.Empty;

	public string PaymentMethod { get; set; } = string.Empty;

	public string ProductName { get; set; } = string.Empty;

	public string StripeCustomerId { get; set; } = string.Empty;

	public string StripePaymentIntentId { get; set; } = string.Empty;

	public string StripePriceId { get; set; } = string.Empty;

	public string StripeSubscriptionId { get; set; } = string.Empty;

	public long? UserId { get; set; }


	#endregion
}
