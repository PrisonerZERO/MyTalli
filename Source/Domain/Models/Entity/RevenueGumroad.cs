namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class RevenueGumroad : DefaultModel
{
	#region <Properties>

	public decimal? DiscoverFee { get; set; }

	public string? LicenseKey { get; set; }

	public string SaleId { get; set; } = string.Empty;

	#endregion
}
