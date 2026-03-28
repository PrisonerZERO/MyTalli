namespace My.Talli.Domain.Entities;

/// <summary>Entity</summary>
public class RevenueManual : DefaultEntity
{
    public string Category { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public int Quantity { get; set; } = 1;
    public Revenue Revenue { get; set; } = null!;
}
