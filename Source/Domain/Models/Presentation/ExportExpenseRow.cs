namespace My.Talli.Domain.Models;

/// <summary>Presentation model — a single row in the expenses CSV export</summary>
public class ExportExpenseRow
{
	#region <Properties>

	public decimal Amount { get; set; }

	public string Category { get; set; } = string.Empty;

	public string Currency { get; set; } = string.Empty;

	public string Description { get; set; } = string.Empty;

	public DateTime ExpenseDate { get; set; }

	public string Platform { get; set; } = string.Empty;

	public string? PlatformTransactionId { get; set; }

	public string ShopName { get; set; } = string.Empty;


	#endregion
}
