namespace My.Talli.Domain.Entities;

/// <summary>Entity</summary>
public class ExpenseManual : DefaultEntity
{
	#region <Properties>

	public Expense Expense { get; set; } = null!;

	public string? Notes { get; set; }

	#endregion
}
