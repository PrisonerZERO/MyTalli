namespace My.Talli.Web.Models;

/// <summary>Sample Data</summary>
public static class ExpenseDataset
{
	#region <Variables>

	private static readonly DailyCache<List<ExpenseItem>> _dashboardCache = new(GenerateDashboard);
	private static readonly DailyCache<List<ExpenseItem>> _manualCache = new(GenerateManual);

	#endregion

	#region <Methods>

	public static List<ExpenseItem> GetDashboardExpenses() => _dashboardCache.Get();

	public static List<ExpenseItem> GetManualExpenses() => _manualCache.Get();

	private static List<ExpenseItem> GenerateDashboard(DateTime today)
	{
		var rnd = new Random(43);
		var stripeProcessing = new[] { "Payment processing — Logo Design", "Payment processing — Web Dev Retainer", "Payment processing — Consulting Call", "Payment processing — Brand Strategy", "Payment processing — Custom Development", "Payment processing — Site Audit", "Payment processing — Landing Page Build" };
		var etsyListings = new[] { "Listing renewal — Handmade Candle Set", "Listing renewal — Ceramic Mug", "Listing renewal — Linen Table Runner", "Listing renewal x5 — Spring collection", "Listing renewal — Pet Portrait", "Listing renewal — Tea Towel" };
		var etsyTransactions = new[] { "Transaction fee — Handmade Candle Set", "Transaction fee — Ceramic Mug", "Transaction fee — Pet Portrait", "Transaction fee — Linen Runner", "Transaction fee — Beeswax Wraps" };
		var etsyAds = new[] { "Offsite ads — January campaign", "Offsite ads — February campaign", "Offsite ads — March campaign", "Offsite ads — Q4 campaign", "Promoted listing — featured" };
		var etsyShipping = new[] { "USPS Priority — Order #4821", "USPS Ground — Order #5102", "USPS Priority — Order #5340", "UPS Ground — Order #5601", "USPS Priority — Order #5783" };
		var gumroadProcessing = new[] { "Platform fee — Procreate Brush Pack", "Platform fee — Notion Template Bundle", "Platform fee — SVG Cut File Library", "Platform fee — Productivity Workbook", "Platform fee — Lightroom Presets" };
		var results = new List<ExpenseItem>();
		var idCounter = -1;

		for (var dayOffset = -800; dayOffset <= -1; dayOffset++)
		{
			var date = today.AddDays(dayOffset);

			var count = rnd.NextDouble() switch
			{
				< 0.75 => 0,
				< 0.92 => 1,
				< 0.98 => 2,
				_ => 3,
			};

			for (var i = 0; i < count; i++)
			{
				var roll = rnd.NextDouble();

				if (roll < 0.30)
					results.Add(new ExpenseItem { Id = idCounter--, Platform = "Stripe", Category = "Processing Fee", Description = stripeProcessing[rnd.Next(stripeProcessing.Length)], Amount = Math.Round((decimal)(2 + rnd.NextDouble() * 35), 2), ExpenseDate = date });
				else if (roll < 0.50)
					results.Add(new ExpenseItem { Id = idCounter--, Platform = "Etsy", Category = "Listing Fee", Description = etsyListings[rnd.Next(etsyListings.Length)], Amount = Math.Round((decimal)(0.20 + rnd.NextDouble() * 0.80), 2), ExpenseDate = date });
				else if (roll < 0.68)
					results.Add(new ExpenseItem { Id = idCounter--, Platform = "Etsy", Category = "Processing Fee", Description = etsyTransactions[rnd.Next(etsyTransactions.Length)], Amount = Math.Round((decimal)(1 + rnd.NextDouble() * 8), 2), ExpenseDate = date });
				else if (roll < 0.78)
					results.Add(new ExpenseItem { Id = idCounter--, Platform = "Etsy", Category = "Ad Fee", Description = etsyAds[rnd.Next(etsyAds.Length)], Amount = Math.Round((decimal)(5 + rnd.NextDouble() * 45), 2), ExpenseDate = date });
				else if (roll < 0.88)
					results.Add(new ExpenseItem { Id = idCounter--, Platform = "Etsy", Category = "Shipping Label", Description = etsyShipping[rnd.Next(etsyShipping.Length)], Amount = Math.Round((decimal)(4 + rnd.NextDouble() * 12), 2), ExpenseDate = date });
				else
					results.Add(new ExpenseItem { Id = idCounter--, Platform = "Gumroad", Category = "Processing Fee", Description = gumroadProcessing[rnd.Next(gumroadProcessing.Length)], Amount = Math.Round((decimal)(0.50 + rnd.NextDouble() * 4), 2), ExpenseDate = date });
			}
		}

		return results;
	}

	private static List<ExpenseItem> GenerateManual(DateTime today)
	{
		var rnd = new Random(46);
		var subscriptions = new[] { ("Canva Pro monthly", 12.99m), ("Adobe Creative Cloud", 54.99m), ("Notion Pro", 8.00m), ("Figma Professional", 15.00m), ("Mailchimp Standard", 20.00m), ("Squarespace hosting", 18.00m) };
		var ads = new[] { "Instagram promoted post", "Facebook ad — candle collection", "Pinterest promoted pin", "TikTok Spark Ad", "Local newspaper ad", "Etsy Offsite Ad reimbursement" };
		var shipping = new[] { "USPS Priority — craft fair order", "USPS Ground — bulk order", "UPS Ground — wholesale shipment", "USPS Flat Rate — pottery box", "FedEx Express — rush order" };
		var processing = new[] { "Square reader fee — market sales", "PayPal fee — invoice", "Venmo Business fee", "Cash App for Business fee" };
		var supplies = new[] { "Craft supplies — wax and wicks", "Packaging materials — boxes and tissue", "Yarn and fiber restock", "Clay and glaze restock", "Print paper and ink", "Shipping labels and tape" };
		var results = new List<ExpenseItem>();
		var idCounter = -1L;

		// Recurring monthly subscriptions — one each on a deterministic day per month
		for (var monthsBack = 13; monthsBack >= 0; monthsBack--)
		{
			var monthAnchor = today.AddMonths(-monthsBack);

			foreach (var (description, amount) in subscriptions)
			{
				var billingDay = Math.Min(DateTime.DaysInMonth(monthAnchor.Year, monthAnchor.Month), 1 + (description.Length % 27));
				var billingDate = new DateTime(monthAnchor.Year, monthAnchor.Month, billingDay);

				if (billingDate < today)
					results.Add(new ExpenseItem { Id = idCounter--, Platform = "Manual", Category = "Subscription Fee", Description = description, Amount = amount, ExpenseDate = billingDate });
			}
		}

		// Occasional one-off expenses across the same window
		for (var dayOffset = -400; dayOffset <= -1; dayOffset++)
		{
			var date = today.AddDays(dayOffset);

			var count = rnd.NextDouble() switch
			{
				< 0.78 => 0,
				< 0.95 => 1,
				_ => 2,
			};

			for (var i = 0; i < count; i++)
			{
				var roll = rnd.NextDouble();

				if (roll < 0.30)
					results.Add(new ExpenseItem { Id = idCounter--, Platform = "Manual", Category = "Ad Fee", Description = ads[rnd.Next(ads.Length)], Amount = Math.Round((decimal)(10 + rnd.NextDouble() * 65), 2), ExpenseDate = date });
				else if (roll < 0.55)
					results.Add(new ExpenseItem { Id = idCounter--, Platform = "Manual", Category = "Shipping Label", Description = shipping[rnd.Next(shipping.Length)], Amount = Math.Round((decimal)(5 + rnd.NextDouble() * 18), 2), ExpenseDate = date });
				else if (roll < 0.70)
					results.Add(new ExpenseItem { Id = idCounter--, Platform = "Manual", Category = "Processing Fee", Description = processing[rnd.Next(processing.Length)], Amount = Math.Round((decimal)(2 + rnd.NextDouble() * 12), 2), ExpenseDate = date });
				else
					results.Add(new ExpenseItem { Id = idCounter--, Platform = "Manual", Category = "Other", Description = supplies[rnd.Next(supplies.Length)], Amount = Math.Round((decimal)(15 + rnd.NextDouble() * 80), 2), ExpenseDate = date });
			}
		}

		return results;
	}

	#endregion
}
