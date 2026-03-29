namespace My.Talli.Domain.Entities;

/// <summary>Entity</summary>
public class RevenueGumroad : DefaultEntity
{
	#region <Properties>

	public decimal? DiscoverFee { get; set; }

	public string? LicenseKey { get; set; }

	public Revenue Revenue { get; set; } = null!;

	public string SaleId { get; set; } = string.Empty;

	#endregion
}
