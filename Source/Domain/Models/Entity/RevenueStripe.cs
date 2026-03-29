namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class RevenueStripe : DefaultModel
{
	#region <Properties>

	public string BalanceTransactionId { get; set; } = string.Empty;

	public decimal? ExchangeRate { get; set; }

	public string PaymentMethod { get; set; } = string.Empty;

	public int? RiskScore { get; set; }

	#endregion
}
