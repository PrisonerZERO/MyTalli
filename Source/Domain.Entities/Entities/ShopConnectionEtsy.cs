namespace My.Talli.Domain.Entities;

/// <summary>Entity</summary>
public class ShopConnectionEtsy : DefaultEntity
{
	#region <Properties>

	public string CountryCode { get; set; } = string.Empty;

	public bool IsVacationMode { get; set; }

	public ShopConnection ShopConnection { get; set; } = null!;

	public string ShopCurrency { get; set; } = string.Empty;

	public string ShopUrl { get; set; } = string.Empty;

	#endregion
}
