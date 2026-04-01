namespace My.Talli.Web.Models;

/// <summary>Model</summary>
public class ExpenseItem
{
	#region <Properties>

	public decimal Amount { get; set; }

	public string Category { get; set; } = "";

	public string CategoryCss => Category switch
	{
		"Processing Fee" => "cat-service",
		"Listing Fee" => "cat-consulting",
		"Ad Fee" => "cat-freelance",
		"Subscription Fee" => "cat-digital",
		"Shipping Label" => "cat-physical",
		_ => "cat-other"
	};

	public string Currency { get; set; } = "USD";

	public string Description { get; set; } = "";

	public DateTime ExpenseDate { get; set; }

	public string ExpenseDateLabel => ExpenseDate.ToString("MMM d, yyyy");

	public long Id { get; set; }

	public string Platform { get; set; } = "";

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

	#endregion
}
