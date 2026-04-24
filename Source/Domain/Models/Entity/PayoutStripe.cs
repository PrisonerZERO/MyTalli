namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class PayoutStripe : DefaultModel
{
	#region <Properties>

	public string PayoutMethod { get; set; } = string.Empty;

	public string? StatementDescriptor { get; set; }

	public string StripePayoutId { get; set; } = string.Empty;

	#endregion
}
