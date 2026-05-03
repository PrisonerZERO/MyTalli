namespace My.Talli.Domain.Models;

/// <summary>Presentation model — counts of rows the user is about to export at the current filter</summary>
public class ExportPreview
{
	#region <Properties>

	public int ExpenseRowCount { get; set; }

	public DateTime FromDateUtc { get; set; }

	public int PayoutRowCount { get; set; }

	public int RevenueRowCount { get; set; }

	public DateTime ToDateUtc { get; set; }

	public int TotalRowCount => RevenueRowCount + ExpenseRowCount + PayoutRowCount;


	#endregion
}
