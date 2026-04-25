namespace My.Talli.Domain.Commands.Platforms;

/// <summary>Input</summary>
public class EtsyExpenseInput
{
    #region <Properties>

    public long? AdCampaignId { get; set; }

    public decimal Amount { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Currency { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime ExpenseDate { get; set; }

    public long LedgerEntryId { get; set; }

    public long? ListingId { get; set; }

    #endregion
}
