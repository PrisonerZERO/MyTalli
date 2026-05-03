namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class Heartbeat : DefaultModel
{
	#region <Properties>

	public int ExpectedIntervalSeconds { get; set; }

	public string HeartbeatSource { get; set; } = string.Empty;

	public DateTime LastTickAt { get; set; }

	public string? Metadata { get; set; }


	#endregion
}
