namespace My.Talli.Web.Models;

/// <summary>Model</summary>
public class PlatformItem
{
	#region <Properties>

	public string BrandColor { get; set; } = "";

	public string ConnectionStatus { get; set; } = "";

	public string Description { get; set; } = "";

	public string Icon { get; set; } = "";

	public bool IsConnected { get; set; }

	public bool IsEnabled { get; set; } = true;

	public string? LastErrorMessage { get; set; }

	public string LastSyncLabel { get; set; } = "";

	public string Name { get; set; } = "";

	public List<ShopItem> Shops { get; set; } = [];

	public string Subtitle { get; set; } = "";

	public string SyncStatus { get; set; } = "";

	public int TransactionCount { get; set; }


	#endregion
}
