namespace My.Talli.Web.Models;

/// <summary>Model</summary>
public class ManualEntryItem
{
	#region <Properties>

	public string Category { get; set; } = "";

	public string CategoryCss => Category switch
	{
		"Sale" => "cat-sale",
		"Service" => "cat-service",
		"Freelance" => "cat-freelance",
		"Consulting" => "cat-consulting",
		"Digital Product" => "cat-digital",
		"Physical Product" => "cat-physical",
		_ => "cat-other"
	};

	public string Currency { get; set; } = "";

	public string Description { get; set; } = "";

	public decimal FeeAmount { get; set; }

	public decimal GrossAmount { get; set; }

	public long Id { get; set; }

	public decimal NetAmount { get; set; }

	public string? Notes { get; set; }

	public int Quantity { get; set; } = 1;

	public DateTime TransactionDate { get; set; }

	public decimal UnitPrice => Quantity > 0 ? GrossAmount / Quantity : GrossAmount;

	public string TransactionDateLabel => TransactionDate.ToString("MMM d, yyyy");

	#endregion
}
