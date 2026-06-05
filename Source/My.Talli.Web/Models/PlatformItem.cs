namespace My.Talli.Web.Models;

/// <summary>Model</summary>
public class PlatformItem
{
	#region <Properties>

	public string BrandColor { get; set; } = "";

	public bool CanAddAnotherShop { get; set; }

	public bool CanConnect { get; set; }

	public string ConnectionStatus { get; set; } = "";

	public string Description { get; set; } = "";

	public string Icon { get; set; } = "";

	public bool IsAvailable { get; set; }

	public bool IsConnected { get; set; }

	public bool IsEnabled { get; set; } = true;

	public string? LastErrorMessage { get; set; }

	public string LastSyncLabel { get; set; } = "";

	public string Name { get; set; } = "";

	public List<ShopItem> Shops { get; set; } = [];

	public string Subtitle { get; set; } = "";

	// Whether the user can connect more than one account/shop on this platform. Etsy is genuinely
	// multi-shop (one login = one shop, so multi-shop sellers use multiple logins). Gumroad has no shop
	// concept (the account IS the seller) and its OAuth offers no account switcher, so "Connect another
	// shop" is hidden for it. Defaults true; set false per-platform in the catalog.
	public bool SupportsMultipleShops { get; set; } = true;

	public string SyncStatus { get; set; } = "";

	public int TransactionCount { get; set; }


	#endregion
}
