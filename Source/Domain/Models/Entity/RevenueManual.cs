namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class RevenueManual : DefaultModel
{
    public string Category { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
