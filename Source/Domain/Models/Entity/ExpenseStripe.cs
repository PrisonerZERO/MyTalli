namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class ExpenseStripe : DefaultModel
{
	#region <Properties>

	public string BalanceTransactionId { get; set; } = string.Empty;

	public string FeeType { get; set; } = string.Empty;

	#endregion
}
