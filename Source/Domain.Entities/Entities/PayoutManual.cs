namespace My.Talli.Domain.Entities;

/// <summary>Entity</summary>
public class PayoutManual : DefaultEntity
{
	#region <Properties>

	public string? Notes { get; set; }

	public Payout Payout { get; set; } = null!;

	#endregion
}
