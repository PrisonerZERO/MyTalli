namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class OrderItem : DefaultModel
{
	#region <Properties>

	public long OrderId { get; set; }

	public long ProductId { get; set; }

	public decimal ProductPriceCharged { get; set; }

	public int ProductQuantity { get; set; }

	#endregion
}
