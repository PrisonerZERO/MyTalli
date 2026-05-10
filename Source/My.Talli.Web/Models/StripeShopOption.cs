namespace My.Talli.Web.Models;

/// <summary>Row Model</summary>
public class StripeShopOption
{
	#region <Properties>

	public string DisplayName { get; set; } = string.Empty;

	public bool IsEnabled { get; set; } = true;

	public int SaleCount { get; set; }

	public long ShopConnectionId { get; set; }

	#endregion
}
