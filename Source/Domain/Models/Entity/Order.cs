namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class Order
{
	#region <Properties>

	public long Id { get; set; }

	public DateTime OrderDateTime { get; set; }

	public decimal TaxCharged { get; set; }

	public long UserId { get; set; }

	#endregion
}
