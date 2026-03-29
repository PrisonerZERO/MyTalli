namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class SyncQueue : DefaultModel
{
	#region <Properties>

	public int ConsecutiveFailures { get; set; }

	public DateTime CreatedOn { get; set; }

	public bool IsEnabled { get; set; } = true;

	public string? LastErrorMessage { get; set; }

	public DateTime? LastSyncDateTime { get; set; }

	public DateTime NextSyncDateTime { get; set; }

	public string Platform { get; set; } = string.Empty;

	public string Status { get; set; } = string.Empty;

	public long UserId { get; set; }

	#endregion
}
