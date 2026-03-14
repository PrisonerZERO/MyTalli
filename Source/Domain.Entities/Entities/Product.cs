namespace My.Talli.Domain.Entities;

/// <summary>Entity</summary>
public class Product : DefaultEntity
{
	#region <Properties>

	public string ProductName { get; set; } = string.Empty;

	public ProductType ProductType { get; set; } = null!;

	public long ProductTypeId { get; set; }

	public ProductVendor ProductVendor { get; set; } = null!;

	public decimal VendorPrice { get; set; }

	public long VendorId { get; set; }

	#endregion
}
