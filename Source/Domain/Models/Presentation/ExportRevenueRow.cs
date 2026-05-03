namespace My.Talli.Domain.Models;

/// <summary>Presentation model — a single row in the revenue CSV export (joined with shop name)</summary>
public class ExportRevenueRow
{
	#region <Properties>

	public string Currency { get; set; } = string.Empty;

	public string Description { get; set; } = string.Empty;

	public decimal FeeAmount { get; set; }

	public decimal GrossAmount { get; set; }

	public bool IsDisputed { get; set; }

	public bool IsRefunded { get; set; }

	public decimal NetAmount { get; set; }

	public string Platform { get; set; } = string.Empty;

	public string? PlatformTransactionId { get; set; }

	public string ShopName { get; set; } = string.Empty;

	public DateTime TransactionDate { get; set; }


	#endregion
}
