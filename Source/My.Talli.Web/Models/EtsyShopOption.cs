namespace My.Talli.Web.Models;

/// <summary>Row Model</summary>
public class EtsyShopOption
{
	#region <Properties>

	public bool IsEnabled { get; set; } = true;

	public int SaleCount { get; set; }

	public long ShopConnectionId { get; set; }

	public string ShopName { get; set; } = string.Empty;

	#endregion
}
