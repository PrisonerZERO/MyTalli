namespace My.Talli.Domain.Entities;

/// <summary>Entity</summary>
public class PayoutEtsy : DefaultEntity
{
	#region <Properties>

	public long LedgerEntryId { get; set; }

	public Payout Payout { get; set; } = null!;

	public string ShopCurrency { get; set; } = string.Empty;

	#endregion
}
