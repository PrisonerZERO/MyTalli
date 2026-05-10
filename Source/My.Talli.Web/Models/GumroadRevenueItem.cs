namespace My.Talli.Web.Models;

/// <summary>Row Model</summary>
public class GumroadRevenueItem
{
	#region <Properties>

	public string Currency { get; set; } = "USD";

	public string Description { get; set; } = string.Empty;

	public decimal FeeAmount { get; set; }

	public decimal GrossAmount { get; set; }

	public long Id { get; set; }

	public bool IsDisputed { get; set; }

	public bool IsRefunded { get; set; }

	public string? LicenseKey { get; set; }

	public decimal NetAmount { get; set; }

	public string SaleId { get; set; } = string.Empty;

	public long? ShopConnectionId { get; set; }

	public string ShopName { get; set; } = string.Empty;

	public string StatusCss => IsDisputed ? "disputed" : IsRefunded ? "refunded" : "paid";

	public string StatusLabel => IsDisputed ? "Disputed" : IsRefunded ? "Refunded" : "Paid";

	public DateTime TransactionDate { get; set; }

	#endregion
}
