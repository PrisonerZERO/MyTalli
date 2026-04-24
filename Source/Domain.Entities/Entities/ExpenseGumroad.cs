namespace My.Talli.Domain.Entities;

/// <summary>Entity</summary>
public class ExpenseGumroad : DefaultEntity
{
	#region <Properties>

	public Expense Expense { get; set; } = null!;

	public string? SubscriptionPlanId { get; set; }

	#endregion
}
