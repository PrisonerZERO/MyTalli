namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class OrderItem
{
	#region <Properties>

	public long Id { get; set; }

	public long OrderId { get; set; }

	public long ProductId { get; set; }

	public decimal ProductPriceCharged { get; set; }

	public int ProductQuantity { get; set; }

	#endregion
}
