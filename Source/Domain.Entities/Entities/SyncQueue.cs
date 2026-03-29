namespace My.Talli.Domain.Entities;

/// <summary>Entity</summary>
public class SyncQueue : DefaultEntity
{
	#region <Properties>

	public int ConsecutiveFailures { get; set; }

	public bool IsEnabled { get; set; } = true;

	public string? LastErrorMessage { get; set; }

	public DateTime? LastSyncDateTime { get; set; }

	public DateTime NextSyncDateTime { get; set; }

	public string Platform { get; set; } = string.Empty;

	public string Status { get; set; } = string.Empty;

	public User User { get; set; } = null!;

	public long UserId { get; set; }

	#endregion
}
