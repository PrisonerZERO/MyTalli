namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class Order : DefaultModel
{
	#region <Properties>

	public DateTime OrderDateTime { get; set; }

	public decimal TaxCharged { get; set; }

	public long UserId { get; set; }


	#endregion
}
