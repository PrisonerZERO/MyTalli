namespace My.Talli.Domain.Models;

/// <summary>Presentation model — a single row in the payouts CSV export</summary>
public class ExportPayoutRow
{
	#region <Properties>

	public decimal Amount { get; set; }

	public string Currency { get; set; } = string.Empty;

	public DateTime? ExpectedArrivalDate { get; set; }

	public DateTime PayoutDate { get; set; }

	public string Platform { get; set; } = string.Empty;

	public string? PlatformPayoutId { get; set; }

	public string ShopName { get; set; } = string.Empty;

	public string Status { get; set; } = string.Empty;


	#endregion
}
