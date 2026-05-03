namespace My.Talli.Domain.Models;

/// <summary>Presentation model — aggregated per-platform metrics across all shops + revenue</summary>
public class PlatformHealthStatus
{
	#region <Properties>

	public int FailingShops { get; set; }

	public bool IsAvailable { get; set; }

	public DateTime? LastSyncAt { get; set; }

	public DateTime? MostRecentRevenueAt { get; set; }

	public string Platform { get; set; } = string.Empty;

	public int RevenueRowCount { get; set; }

	public decimal RevenueTotal { get; set; }

	public int StaleShops { get; set; }

	public int TotalShops { get; set; }


	#endregion
}
