namespace My.Talli.Web.Models;

/// <summary>Sample Data</summary>
public static class PayoutDataset
{
	#region <Methods>

	public static List<PayoutItem> GetDashboardPayouts()
	{
		var today = DateTime.Today;

		return
		[
			new PayoutItem { Id = -1, Platform = "Stripe", Amount = 1250.00m, PayoutDate = today.AddDays(-1), ExpectedArrivalDate = today.AddDays(1), Status = "In Transit" },
			new PayoutItem { Id = -2, Platform = "Etsy", Amount = 487.32m, PayoutDate = today.AddDays(-3), ExpectedArrivalDate = today.AddDays(-1), Status = "Paid" },
			new PayoutItem { Id = -3, Platform = "Gumroad", Amount = 156.80m, PayoutDate = today.AddDays(-7), ExpectedArrivalDate = today.AddDays(-5), Status = "Paid" },
			new PayoutItem { Id = -4, Platform = "Stripe", Amount = 890.00m, PayoutDate = today.AddDays(-8), ExpectedArrivalDate = today.AddDays(-6), Status = "Paid" },
			new PayoutItem { Id = -5, Platform = "Etsy", Amount = 312.45m, PayoutDate = today.AddDays(-14), ExpectedArrivalDate = today.AddDays(-12), Status = "Paid" },
			new PayoutItem { Id = -6, Platform = "Stripe", Amount = 675.00m, PayoutDate = today.AddDays(-15), ExpectedArrivalDate = today.AddDays(-13), Status = "Paid" },
			new PayoutItem { Id = -7, Platform = "Gumroad", Amount = 89.20m, PayoutDate = today.AddDays(-21), ExpectedArrivalDate = today.AddDays(-19), Status = "Paid" },
			new PayoutItem { Id = -8, Platform = "Stripe", Amount = 1100.00m, PayoutDate = today.AddDays(-22), ExpectedArrivalDate = today.AddDays(-20), Status = "Paid" },
		];
	}

	public static List<PayoutItem> GetManualPayouts()
	{
		var today = DateTime.Today;

		return
		[
			new PayoutItem { Id = -1, Platform = "Manual", Amount = 807.50m, PayoutDate = today.AddDays(-2), Status = "Paid" },
			new PayoutItem { Id = -2, Platform = "Manual", Amount = 1140.00m, PayoutDate = today.AddDays(-5), Status = "Paid" },
			new PayoutItem { Id = -3, Platform = "Manual", Amount = 570.00m, PayoutDate = today.AddDays(-9), Status = "Pending" },
			new PayoutItem { Id = -4, Platform = "Manual", Amount = 185.00m, PayoutDate = today.AddDays(-12), Status = "Paid" },
			new PayoutItem { Id = -5, Platform = "Manual", Amount = 475.00m, PayoutDate = today.AddDays(-16), Status = "Paid" },
			new PayoutItem { Id = -6, Platform = "Manual", Amount = 304.00m, PayoutDate = today.AddDays(-20), Status = "Paid" },
		];
	}

	#endregion
}
