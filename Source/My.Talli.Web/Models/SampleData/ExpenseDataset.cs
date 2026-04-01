namespace My.Talli.Web.Models;

/// <summary>Sample Data</summary>
public static class ExpenseDataset
{
	#region <Methods>

	public static List<ExpenseItem> GetDashboardExpenses()
	{
		var today = DateTime.Today;

		return
		[
			new ExpenseItem { Id = -1, Platform = "Stripe", Category = "Processing Fee", Description = "Payment processing — Logo Design", Amount = 13.34m, ExpenseDate = today.AddDays(-1) },
			new ExpenseItem { Id = -2, Platform = "Etsy", Category = "Listing Fee", Description = "Listing renewal — Handmade Candle Set", Amount = 0.20m, ExpenseDate = today.AddDays(-2) },
			new ExpenseItem { Id = -3, Platform = "Etsy", Category = "Processing Fee", Description = "Transaction fee — Ceramic Mug", Amount = 2.21m, ExpenseDate = today.AddDays(-3) },
			new ExpenseItem { Id = -4, Platform = "Gumroad", Category = "Processing Fee", Description = "Platform fee — Procreate Brush Pack", Amount = 1.20m, ExpenseDate = today.AddDays(-5) },
			new ExpenseItem { Id = -5, Platform = "Stripe", Category = "Processing Fee", Description = "Payment processing — Web Dev Retainer", Amount = 23.50m, ExpenseDate = today.AddDays(-6) },
			new ExpenseItem { Id = -6, Platform = "Etsy", Category = "Ad Fee", Description = "Offsite ads — March campaign", Amount = 18.40m, ExpenseDate = today.AddDays(-8) },
			new ExpenseItem { Id = -7, Platform = "Etsy", Category = "Listing Fee", Description = "Listing renewal x5 — Spring collection", Amount = 1.00m, ExpenseDate = today.AddDays(-10) },
			new ExpenseItem { Id = -8, Platform = "Stripe", Category = "Processing Fee", Description = "Payment processing — Consulting retainer", Amount = 35.28m, ExpenseDate = today.AddDays(-12) },
			new ExpenseItem { Id = -9, Platform = "Gumroad", Category = "Processing Fee", Description = "Platform fee — Notion Template Bundle", Amount = 2.90m, ExpenseDate = today.AddDays(-14) },
			new ExpenseItem { Id = -10, Platform = "Etsy", Category = "Shipping Label", Description = "USPS Priority — Order #4821", Amount = 8.75m, ExpenseDate = today.AddDays(-16) },
		];
	}

	public static List<ExpenseItem> GetManualExpenses()
	{
		var today = DateTime.Today;

		return
		[
			new ExpenseItem { Id = -1, Platform = "Manual", Category = "Subscription Fee", Description = "Canva Pro monthly", Amount = 12.99m, ExpenseDate = today.AddDays(-2) },
			new ExpenseItem { Id = -2, Platform = "Manual", Category = "Ad Fee", Description = "Instagram promoted post — March", Amount = 25.00m, ExpenseDate = today.AddDays(-5) },
			new ExpenseItem { Id = -3, Platform = "Manual", Category = "Shipping Label", Description = "USPS Priority — craft fair order", Amount = 8.75m, ExpenseDate = today.AddDays(-8) },
			new ExpenseItem { Id = -4, Platform = "Manual", Category = "Processing Fee", Description = "Square reader fee — market sales", Amount = 5.55m, ExpenseDate = today.AddDays(-10) },
			new ExpenseItem { Id = -5, Platform = "Manual", Category = "Subscription Fee", Description = "Adobe Creative Cloud", Amount = 54.99m, ExpenseDate = today.AddDays(-15) },
			new ExpenseItem { Id = -6, Platform = "Manual", Category = "Other", Description = "Craft supplies — wax and wicks", Amount = 42.30m, ExpenseDate = today.AddDays(-18) },
			new ExpenseItem { Id = -7, Platform = "Manual", Category = "Ad Fee", Description = "Facebook ad — candle collection", Amount = 15.00m, ExpenseDate = today.AddDays(-22) },
			new ExpenseItem { Id = -8, Platform = "Manual", Category = "Other", Description = "Packaging materials — boxes and tissue", Amount = 28.50m, ExpenseDate = today.AddDays(-25) },
		];
	}

	#endregion
}
