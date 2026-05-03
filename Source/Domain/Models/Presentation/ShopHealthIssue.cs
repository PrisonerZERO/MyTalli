namespace My.Talli.Domain.Models;

/// <summary>Presentation model — one row per shop that has a sync problem worth surfacing on the admin dashboard</summary>
public class ShopHealthIssue
{
	#region <Properties>

	public int ConsecutiveFailures { get; set; }

	public string IssueKind { get; set; } = string.Empty;

	public string? LastErrorMessage { get; set; }

	public DateTime? LastSyncDateTime { get; set; }

	public DateTime NextSyncDateTime { get; set; }

	public string Platform { get; set; } = string.Empty;

	public long ShopConnectionId { get; set; }

	public string ShopName { get; set; } = string.Empty;

	public long UserId { get; set; }


	#endregion
}
