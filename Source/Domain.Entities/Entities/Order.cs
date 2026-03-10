namespace My.Talli.Domain.Entities.Entities;

/// <summary>Entity</summary>
public class Order : DefaultEntity
{
	#region <Properties>

	public DateTime OrderDateTime { get; set; }

	public decimal TaxCharged { get; set; }

	public User User { get; set; } = null!;

	public long UserId { get; set; }

	#endregion
}
