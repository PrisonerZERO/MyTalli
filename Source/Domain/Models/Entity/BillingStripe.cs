namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class BillingStripe : DefaultModel
{
	#region <Properties>

	public string CardBrand { get; set; } = string.Empty;

	public string CardLastFour { get; set; } = string.Empty;

	public string PaymentMethod { get; set; } = string.Empty;

	public string StripePaymentIntentId { get; set; } = string.Empty;

	#endregion
}
