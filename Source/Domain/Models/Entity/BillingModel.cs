namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class BillingModel
{
	#region <Properties>

	public decimal Amount { get; set; }

	public string Currency { get; set; } = string.Empty;

	public long Id { get; set; }

	public long OrderId { get; set; }

	public string Status { get; set; } = string.Empty;

	public long UserId { get; set; }

	#endregion
}
