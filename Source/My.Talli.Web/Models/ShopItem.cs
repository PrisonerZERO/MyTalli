namespace My.Talli.Web.Models;

/// <summary>Model</summary>
public class ShopItem
{
	#region <Properties>

	public int ConsecutiveFailures { get; set; }

	public string ConnectionStatus { get; set; } = "";

	public string FriendlyHealthMessage { get; set; } = "";

	public ShopHealth Health { get; set; } = ShopHealth.Healthy;

	public bool IsEnabled { get; set; } = true;

	public bool IsOAuthPlatform { get; set; }

	public string? LastErrorMessage { get; set; }

	public string LastSyncLabel { get; set; } = "";

	public string Platform { get; set; } = "";

	public long ShopConnectionId { get; set; }

	public string ShopName { get; set; } = "";

	public string SyncStatus { get; set; } = "";

	public int TransactionCount { get; set; }

	#endregion
}
