namespace My.Talli.Web.Models;

/// <summary>Row Model</summary>
public class EtsyPayoutItem
{
	#region <Properties>

	public decimal Amount { get; set; }

	public string Currency { get; set; } = "USD";

	public DateTime? ExpectedArrivalDate { get; set; }

	public long Id { get; set; }

	public long? LedgerEntryId { get; set; }

	public DateTime PayoutDate { get; set; }

	public long? ShopConnectionId { get; set; }

	public string ShopName { get; set; } = string.Empty;

	public string Status { get; set; } = string.Empty;

	public string StatusCss => Status switch
	{
		"Paid" => "paid",
		"In Transit" => "intransit",
		"Pending" => "pending",
		"Failed" => "disputed",
		"Cancelled" => "refunded",
		_ => "other"
	};

	#endregion
}
