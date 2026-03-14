namespace My.Talli.Domain.Entities;

/// <summary>Entity</summary>
public class Billing : DefaultEntity
{
	#region <Properties>

	public decimal Amount { get; set; }

	public string Currency { get; set; } = string.Empty;

	public Order Order { get; set; } = null!;

	public long OrderId { get; set; }

	public string Status { get; set; } = string.Empty;

	public User User { get; set; } = null!;

	public long UserId { get; set; }

	#endregion
}
