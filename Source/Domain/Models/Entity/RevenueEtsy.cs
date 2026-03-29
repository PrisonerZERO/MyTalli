namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class RevenueEtsy : DefaultModel
{
	#region <Properties>

	public decimal? AdjustedFees { get; set; }

	public decimal? AdjustedGross { get; set; }

	public decimal? AdjustedNet { get; set; }

	public long ListingId { get; set; }

	public long ReceiptId { get; set; }

	public string ShopCurrency { get; set; } = string.Empty;

	#endregion
}
