namespace My.Talli.Domain.Entities;

/// <summary>Entity</summary>
public class Payout : DefaultEntity
{
	#region <Properties>

	public decimal Amount { get; set; }

	public string Currency { get; set; } = string.Empty;

	public DateTime? ExpectedArrivalDate { get; set; }

	public DateTime PayoutDate { get; set; }

	public string Platform { get; set; } = string.Empty;

	public string PlatformPayoutId { get; set; } = string.Empty;

	public string Status { get; set; } = string.Empty;

	public User User { get; set; } = null!;

	public long UserId { get; set; }

	#endregion
}
