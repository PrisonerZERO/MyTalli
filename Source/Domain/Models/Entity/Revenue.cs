namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class Revenue : DefaultModel
{
    public string Currency { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal FeeAmount { get; set; }
    public decimal GrossAmount { get; set; }
    public bool IsDisputed { get; set; }
    public bool IsRefunded { get; set; }
    public decimal NetAmount { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string? PlatformTransactionId { get; set; }
    public DateTime TransactionDate { get; set; }
    public long UserId { get; set; }
}
