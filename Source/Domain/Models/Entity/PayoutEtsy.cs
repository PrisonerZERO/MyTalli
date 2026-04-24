namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class PayoutEtsy : DefaultModel
{
	#region <Properties>

	public long LedgerEntryId { get; set; }

	public string ShopCurrency { get; set; } = string.Empty;

	#endregion
}
