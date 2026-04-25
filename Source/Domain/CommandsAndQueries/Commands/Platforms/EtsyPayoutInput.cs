namespace My.Talli.Domain.Commands.Platforms;

/// <summary>Input</summary>
public class EtsyPayoutInput
{
    #region <Properties>

    public decimal Amount { get; set; }

    public string Currency { get; set; } = string.Empty;

    public long LedgerEntryId { get; set; }

    public DateTime PayoutDate { get; set; }

    public string ShopCurrency { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    #endregion
}
