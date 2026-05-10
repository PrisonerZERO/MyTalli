namespace My.Talli.Web.Models;

/// <summary>Row Model</summary>
public class StripePayoutItem
{
	#region <Properties>

	public decimal Amount { get; set; }

	public string Currency { get; set; } = "USD";

	public DateTime? ExpectedArrivalDate { get; set; }

	public long Id { get; set; }

	public DateTime PayoutDate { get; set; }

	public string PayoutMethod { get; set; } = string.Empty;

	public long? ShopConnectionId { get; set; }

	public string ShopName { get; set; } = string.Empty;

	public string? StatementDescriptor { get; set; }

	public string Status { get; set; } = string.Empty;

	public string StatusCss => Status switch
	{
		"Paid" or "paid" => "paid",
		"In Transit" or "in_transit" => "intransit",
		"Pending" or "pending" => "pending",
		"Failed" or "failed" => "disputed",
		"Cancelled" or "canceled" => "refunded",
		_ => "other"
	};

	public string StatusLabel => Status switch
	{
		"in_transit" => "In Transit",
		"paid" => "Paid",
		"pending" => "Pending",
		"failed" => "Failed",
		"canceled" => "Cancelled",
		_ => Status
	};

	public string StripePayoutId { get; set; } = string.Empty;

	#endregion
}
