namespace My.Talli.Domain.Commands.Platforms;

/// <summary>Input</summary>
public class StripeRevenueInput
{
    #region <Properties>

    public string BalanceTransactionId { get; set; } = string.Empty;

    public string Currency { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal? ExchangeRate { get; set; }

    public decimal FeeAmount { get; set; }

    public decimal GrossAmount { get; set; }

    public bool IsDisputed { get; set; }

    public bool IsRefunded { get; set; }

    public decimal NetAmount { get; set; }

    public string PaymentMethod { get; set; } = string.Empty;

    public int? RiskScore { get; set; }

    public DateTime TransactionDate { get; set; }

    #endregion
}
