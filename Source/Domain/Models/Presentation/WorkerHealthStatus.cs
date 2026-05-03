namespace My.Talli.Domain.Models;

/// <summary>Presentation model — one row per background worker source in app.Heartbeat</summary>
public class WorkerHealthStatus
{
	#region <Properties>

	public int ExpectedIntervalSeconds { get; set; }

	public bool HasTicked { get; set; }

	public bool IsStale { get; set; }

	public DateTime? LastTickAt { get; set; }

	public string? Metadata { get; set; }

	public TimeSpan? SecondsSinceLastTick { get; set; }

	public string Source { get; set; } = string.Empty;


	#endregion
}
