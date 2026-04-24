namespace My.Talli.Domain.Entities;

/// <summary>Entity</summary>
public class ExpenseEtsy : DefaultEntity
{
	#region <Properties>

	public long? AdCampaignId { get; set; }

	public Expense Expense { get; set; } = null!;

	public long LedgerEntryId { get; set; }

	public long? ListingId { get; set; }

	#endregion
}
