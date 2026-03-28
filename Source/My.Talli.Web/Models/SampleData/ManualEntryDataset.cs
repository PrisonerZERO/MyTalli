namespace My.Talli.Web.Models;

/// <summary>Sample Data</summary>
public static class ManualEntryDataset
{
	#region <Methods>

	public static List<ManualEntryItem> GetEntries()
	{
		var today = DateTime.Today;

		return
		[
			new ManualEntryItem { Id = -1, Description = "Logo design for startup", Category = "Freelance", GrossAmount = 850.00m, FeeAmount = 42.50m, NetAmount = 807.50m, Quantity = 1, Currency = "USD", TransactionDate = today.AddDays(-1), Notes = "Branding package for TechNova" },
			new ManualEntryItem { Id = -2, Description = "Monthly consulting retainer", Category = "Consulting", GrossAmount = 1200.00m, FeeAmount = 60.00m, NetAmount = 1140.00m, Quantity = 1, Currency = "USD", TransactionDate = today.AddDays(-3) },
			new ManualEntryItem { Id = -3, Description = "UI template pack", Category = "Digital Product", GrossAmount = 45.00m, FeeAmount = 4.50m, NetAmount = 40.50m, Quantity = 3, Currency = "USD", TransactionDate = today.AddDays(-6), Notes = "Sold on personal site" },
			new ManualEntryItem { Id = -4, Description = "Workshop facilitation", Category = "Service", GrossAmount = 600.00m, FeeAmount = 30.00m, NetAmount = 570.00m, Quantity = 2, Currency = "USD", TransactionDate = today.AddDays(-8) },
			new ManualEntryItem { Id = -5, Description = "Handmade candles (market)", Category = "Sale", GrossAmount = 185.00m, FeeAmount = 0m, NetAmount = 185.00m, Quantity = 37, Currency = "USD", TransactionDate = today.AddDays(-10) },
			new ManualEntryItem { Id = -6, Description = "Brand strategy session", Category = "Consulting", GrossAmount = 500.00m, FeeAmount = 25.00m, NetAmount = 475.00m, Quantity = 1, Currency = "USD", TransactionDate = today.AddDays(-12) },
			new ManualEntryItem { Id = -7, Description = "Icon set commission", Category = "Freelance", GrossAmount = 320.00m, FeeAmount = 16.00m, NetAmount = 304.00m, Quantity = 1, Currency = "USD", TransactionDate = today.AddDays(-15), Notes = "40 custom icons for SaaS dashboard" },
			new ManualEntryItem { Id = -8, Description = "Photography editing batch", Category = "Service", GrossAmount = 280.00m, FeeAmount = 14.00m, NetAmount = 266.00m, Quantity = 14, Currency = "USD", TransactionDate = today.AddDays(-18) },
			new ManualEntryItem { Id = -9, Description = "Notion template bundle", Category = "Digital Product", GrossAmount = 29.00m, FeeAmount = 2.90m, NetAmount = 26.10m, Quantity = 1, Currency = "USD", TransactionDate = today.AddDays(-20) },
			new ManualEntryItem { Id = -10, Description = "Craft fair earnings", Category = "Sale", GrossAmount = 221.00m, FeeAmount = 0m, NetAmount = 221.00m, Quantity = 12, Currency = "USD", TransactionDate = today.AddDays(-22) },
			new ManualEntryItem { Id = -11, Description = "SEO audit report", Category = "Consulting", GrossAmount = 750.00m, FeeAmount = 37.50m, NetAmount = 712.50m, Quantity = 1, Currency = "USD", TransactionDate = today.AddDays(-25) },
			new ManualEntryItem { Id = -12, Description = "Wedding invitation design", Category = "Freelance", GrossAmount = 400.00m, FeeAmount = 20.00m, NetAmount = 380.00m, Quantity = 1, Currency = "USD", TransactionDate = today.AddDays(-28), Notes = "Rush order, premium rate" },
		];
	}

	#endregion
}
