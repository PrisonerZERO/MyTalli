namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class ProductModel
{
	#region <Properties>

	public long Id { get; set; }

	public string ProductName { get; set; } = string.Empty;

	public long ProductTypeId { get; set; }

	public long VendorId { get; set; }

	public decimal VendorPrice { get; set; }

	#endregion
}
