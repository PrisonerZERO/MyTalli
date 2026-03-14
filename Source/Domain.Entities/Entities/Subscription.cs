namespace My.Talli.Domain.Entities;

/// <summary>Entity</summary>
public class Subscription : DefaultEntity
{
	#region <Properties>

	public DateTime CancelledDate { get; set; }

	public DateTime EndDate { get; set; }

	public OrderItem OrderItem { get; set; } = null!;

	public long OrderItemId { get; set; }

	public Product Product { get; set; } = null!;

	public long ProductId { get; set; }

	public DateTime RenewalDate { get; set; }

	public DateTime StartDate { get; set; }

	public string Status { get; set; } = string.Empty;

	public User User { get; set; } = null!;

	public long UserId { get; set; }

	#endregion
}
