namespace My.Talli.Domain.Entities;

/// <summary>Entity</summary>
public class PayoutGumroad : DefaultEntity
{
	#region <Properties>

	public Payout Payout { get; set; } = null!;

	public string PayoutMethod { get; set; } = string.Empty;

	#endregion
}
