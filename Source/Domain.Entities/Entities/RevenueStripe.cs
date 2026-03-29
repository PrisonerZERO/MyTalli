namespace My.Talli.Domain.Entities;

/// <summary>Entity</summary>
public class RevenueStripe : DefaultEntity
{
	#region <Properties>

	public string BalanceTransactionId { get; set; } = string.Empty;

	public decimal? ExchangeRate { get; set; }

	public string PaymentMethod { get; set; } = string.Empty;

	public Revenue Revenue { get; set; } = null!;

	public int? RiskScore { get; set; }

	#endregion
}
