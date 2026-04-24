namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class ExpenseEtsy : DefaultModel
{
	#region <Properties>

	public long? AdCampaignId { get; set; }

	public long LedgerEntryId { get; set; }

	public long? ListingId { get; set; }

	#endregion
}
