namespace My.Talli.Domain.Commands.Platforms;

/// <summary>Input</summary>
public class GumroadRevenueInput
{
    #region <Properties>

    public string Currency { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal? DiscoverFee { get; set; }

    public decimal FeeAmount { get; set; }

    public decimal GrossAmount { get; set; }

    public bool IsDisputed { get; set; }

    public bool IsRefunded { get; set; }

    public string? LicenseKey { get; set; }

    public decimal NetAmount { get; set; }

    public string SaleId { get; set; } = string.Empty;

    public DateTime TransactionDate { get; set; }

    #endregion
}
