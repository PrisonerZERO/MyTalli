namespace My.Talli.Domain.Commands.Platforms;

/// <summary>Input</summary>
public class EtsyRevenueInput
{
    #region <Properties>

    public string Currency { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal FeeAmount { get; set; }

    public decimal GrossAmount { get; set; }

    public long ListingId { get; set; }

    public decimal NetAmount { get; set; }

    public long ReceiptId { get; set; }

    public string ShopCurrency { get; set; } = string.Empty;

    public DateTime TransactionDate { get; set; }

    public long TransactionId { get; set; }

    #endregion
}
