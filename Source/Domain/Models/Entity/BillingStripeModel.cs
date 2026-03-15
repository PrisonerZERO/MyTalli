namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class BillingStripeModel
{
	#region <Properties>

	public long BillingId { get; set; }

	public string CardBrand { get; set; } = string.Empty;

	public string CardLastFour { get; set; } = string.Empty;

	public long Id { get; set; }

	public string PaymentMethod { get; set; } = string.Empty;

	public string StripePaymentIntentId { get; set; } = string.Empty;

	#endregion
}
