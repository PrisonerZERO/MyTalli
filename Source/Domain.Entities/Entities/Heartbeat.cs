namespace My.Talli.Domain.Entities;

/// <summary>Entity</summary>
public class Heartbeat : DefaultEntity
{
	#region <Properties>

	public int ExpectedIntervalSeconds { get; set; }

	public string HeartbeatSource { get; set; } = string.Empty;

	public DateTime LastTickAt { get; set; }

	public string? Metadata { get; set; }


	#endregion
}
