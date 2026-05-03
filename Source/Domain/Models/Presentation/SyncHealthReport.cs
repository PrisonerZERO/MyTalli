namespace My.Talli.Domain.Models;

/// <summary>Presentation model — aggregate snapshot used by the admin Sync Health tab</summary>
public class SyncHealthReport
{
	#region <Properties>

	public DateTime GeneratedAt { get; set; }

	public List<ShopHealthIssue> ProblemShops { get; set; } = [];

	public List<PlatformHealthStatus> Platforms { get; set; } = [];

	public List<WorkerHealthStatus> Workers { get; set; } = [];


	#endregion
}
