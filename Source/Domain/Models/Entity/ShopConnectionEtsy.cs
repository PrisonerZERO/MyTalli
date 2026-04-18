namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class ShopConnectionEtsy : DefaultModel
{
	#region <Properties>

	public string CountryCode { get; set; } = string.Empty;

	public bool IsVacationMode { get; set; }

	public string ShopCurrency { get; set; } = string.Empty;

	public string ShopUrl { get; set; } = string.Empty;

	#endregion
}
