namespace My.Talli.Domain.Entities;

/// <summary>Entity</summary>
public class RevenueEtsy : DefaultEntity
{
	#region <Properties>

	public decimal? AdjustedFees { get; set; }

	public decimal? AdjustedGross { get; set; }

	public decimal? AdjustedNet { get; set; }

	public long ListingId { get; set; }

	public long ReceiptId { get; set; }

	public Revenue Revenue { get; set; } = null!;

	public string ShopCurrency { get; set; } = string.Empty;

	#endregion
}
