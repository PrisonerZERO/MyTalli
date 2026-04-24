namespace My.Talli.Web.Models;

/// <summary>Row Model</summary>
public class EtsyExpenseItem
{
	#region <Properties>

	public long? AdCampaignId { get; set; }

	public decimal Amount { get; set; }

	public string Category { get; set; } = string.Empty;

	public string CategoryCss => Category switch
	{
		"Ad Fee" => "ad",
		"Listing Fee" => "listing",
		"Subscription Fee" => "subscription",
		_ => "other"
	};

	public string Currency { get; set; } = "USD";

	public string Description { get; set; } = string.Empty;

	public DateTime ExpenseDate { get; set; }

	public long Id { get; set; }

	public long? LedgerEntryId { get; set; }

	public long? ListingId { get; set; }

	public long? ShopConnectionId { get; set; }

	public string ShopName { get; set; } = string.Empty;

	#endregion
}
