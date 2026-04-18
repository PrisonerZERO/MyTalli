namespace My.Talli.Web.Models;

/// <summary>Model</summary>
public class ShopItem
{
	#region <Properties>

	public string ConnectionStatus { get; set; } = "";

	public bool IsEnabled { get; set; } = true;

	public string? LastErrorMessage { get; set; }

	public string LastSyncLabel { get; set; } = "";

	public long ShopConnectionId { get; set; }

	public string ShopName { get; set; } = "";

	public string SyncStatus { get; set; } = "";

	public int TransactionCount { get; set; }

	#endregion
}
