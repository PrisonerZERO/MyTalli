namespace My.Talli.Domain.Entities;

/// <summary>Entity</summary>
public class PayoutStripe : DefaultEntity
{
	#region <Properties>

	public Payout Payout { get; set; } = null!;

	public string PayoutMethod { get; set; } = string.Empty;

	public string? StatementDescriptor { get; set; }

	public string StripePayoutId { get; set; } = string.Empty;

	#endregion
}
