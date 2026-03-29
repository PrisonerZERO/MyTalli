namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class Payout : DefaultModel
{
	#region <Properties>

	public decimal Amount { get; set; }

	public DateTime CreatedOn { get; set; }

	public string Currency { get; set; } = string.Empty;

	public DateTime? ExpectedArrivalDate { get; set; }

	public DateTime PayoutDate { get; set; }

	public string Platform { get; set; } = string.Empty;

	public string PlatformPayoutId { get; set; } = string.Empty;

	public string Status { get; set; } = string.Empty;

	public long UserId { get; set; }

	#endregion
}
