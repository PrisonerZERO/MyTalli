namespace My.Talli.Domain.Entities;

/// <summary>Entity</summary>
public class OrderItem : DefaultEntity
{
	#region <Properties>

	public Order Order { get; set; } = null!;

	public long OrderId { get; set; }

	public Product Product { get; set; } = null!;

	public long ProductId { get; set; }

	public decimal ProductPriceCharged { get; set; }

	public int ProductQuantity { get; set; }

	#endregion
}
