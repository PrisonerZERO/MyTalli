namespace My.Talli.Domain.Entities.Entities;

/// <summary>Entity</summary>
public class BillingStripe : DefaultEntity
{
	#region <Properties>

	public Billing Billing { get; set; } = null!;

	public long BillingId { get; set; }

	public string CardBrand { get; set; } = string.Empty;

	public string CardLastFour { get; set; } = string.Empty;

	public string PaymentMethod { get; set; } = string.Empty;

	public string StripePaymentIntentId { get; set; } = string.Empty;

	#endregion
}
