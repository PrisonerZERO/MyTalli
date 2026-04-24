namespace My.Talli.Domain.Entities;

/// <summary>Entity</summary>
public class ExpenseStripe : DefaultEntity
{
	#region <Properties>

	public string BalanceTransactionId { get; set; } = string.Empty;

	public Expense Expense { get; set; } = null!;

	public string FeeType { get; set; } = string.Empty;

	#endregion
}
