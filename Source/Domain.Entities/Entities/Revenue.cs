namespace My.Talli.Domain.Entities;

/// <summary>Entity</summary>
public class Revenue : DefaultEntity
{
    public string Currency { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal FeeAmount { get; set; }
    public decimal GrossAmount { get; set; }
    public bool IsDisputed { get; set; }
    public bool IsRefunded { get; set; }
    public decimal NetAmount { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string PlatformTransactionId { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public User User { get; set; } = null!;
    public long UserId { get; set; }
}
