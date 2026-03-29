namespace My.Talli.Domain.Entities;

/// <summary>Entity</summary>
public class Expense : DefaultEntity
{
	#region <Properties>

	public decimal Amount { get; set; }

	public string Category { get; set; } = string.Empty;

	public string Currency { get; set; } = string.Empty;

	public string Description { get; set; } = string.Empty;

	public DateTime ExpenseDate { get; set; }

	public string Platform { get; set; } = string.Empty;

	public string? PlatformTransactionId { get; set; }

	public User User { get; set; } = null!;

	public long UserId { get; set; }

	#endregion
}
