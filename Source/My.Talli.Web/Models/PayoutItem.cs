namespace My.Talli.Web.Models;

/// <summary>Model</summary>
public class PayoutItem
{
	#region <Properties>

	public decimal Amount { get; set; }

	public string Currency { get; set; } = "USD";

	public DateTime? ExpectedArrivalDate { get; set; }

	public string ExpectedArrivalLabel => ExpectedArrivalDate?.ToString("MMM d, yyyy") ?? "\u2014";

	public long Id { get; set; }

	public string Platform { get; set; } = "";

	public DateTime PayoutDate { get; set; }

	public string PayoutDateLabel => PayoutDate.ToString("MMM d, yyyy");

	public string PlatformColor => Platform switch
	{
		"Stripe" => "#635bff",
		"Etsy" => "#f56400",
		"Gumroad" => "#ff90e8",
		"PayPal" => "var(--paypal-color)",
		"Shopify" => "#96bf48",
		"Manual" => "var(--purple-lavender)",
		_ => "var(--text-muted)"
	};

	public string Status { get; set; } = "Paid";

	public string StatusCss => Status switch
	{
		"Paid" => "payout-paid",
		"InTransit" or "In Transit" => "payout-transit",
		"Pending" => "payout-pending",
		"Failed" => "payout-failed",
		"Cancelled" => "payout-cancelled",
		_ => ""
	};

	#endregion
}
