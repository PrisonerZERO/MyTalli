namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class Expense : DefaultModel
{
	#region <Properties>

	public decimal Amount { get; set; }

	public string Category { get; set; } = string.Empty;

	public DateTime CreatedOn { get; set; }

	public string Currency { get; set; } = string.Empty;

	public string Description { get; set; } = string.Empty;

	public DateTime ExpenseDate { get; set; }

	public string Platform { get; set; } = string.Empty;

	public string? PlatformTransactionId { get; set; }

	public long UserId { get; set; }

	#endregion
}
