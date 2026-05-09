namespace My.Talli.Web.Models;

/// <summary>Sample Data</summary>
public static class PayoutDataset
{
	#region <Variables>

	private static readonly DailyCache<List<PayoutItem>> _dashboardCache = new(GenerateDashboard);
	private static readonly DailyCache<List<PayoutItem>> _manualCache = new(GenerateManual);

	#endregion

	#region <Methods>

	public static List<PayoutItem> GetDashboardPayouts() => _dashboardCache.Get();

	public static List<PayoutItem> GetManualPayouts() => _manualCache.Get();

	private static List<PayoutItem> GenerateDashboard(DateTime today)
	{
		var rnd = new Random(44);
		var results = new List<PayoutItem>();
		var idCounter = -1;

		for (var dayOffset = -800; dayOffset <= -1; dayOffset++)
		{
			var date = today.AddDays(dayOffset);

			var count = rnd.NextDouble() switch
			{
				< 0.85 => 0,
				< 0.97 => 1,
				_ => 2,
			};

			for (var i = 0; i < count; i++)
			{
				var roll = rnd.NextDouble();
				var status = dayOffset >= -2 && rnd.NextDouble() < 0.5 ? "In Transit" : "Paid";

				if (roll < 0.50)
					results.Add(new PayoutItem { Id = idCounter--, Platform = "Stripe", Amount = Math.Round((decimal)(300 + rnd.NextDouble() * 1700), 2), PayoutDate = date, ExpectedArrivalDate = date.AddDays(2), Status = status });
				else if (roll < 0.80)
					results.Add(new PayoutItem { Id = idCounter--, Platform = "Etsy", Amount = Math.Round((decimal)(100 + rnd.NextDouble() * 700), 2), PayoutDate = date, ExpectedArrivalDate = date.AddDays(2), Status = status });
				else
					results.Add(new PayoutItem { Id = idCounter--, Platform = "Gumroad", Amount = Math.Round((decimal)(50 + rnd.NextDouble() * 250), 2), PayoutDate = date, ExpectedArrivalDate = date.AddDays(2), Status = status });
			}
		}

		return results;
	}

	private static List<PayoutItem> GenerateManual(DateTime today)
	{
		var rnd = new Random(47);
		var results = new List<PayoutItem>();
		var idCounter = -1L;

		// Roughly weekly payouts (every 6–8 days) over the last ~13 months
		var dayOffset = -2;

		while (dayOffset >= -400)
		{
			var date = today.AddDays(dayOffset);
			var amount = Math.Round((decimal)(150 + rnd.NextDouble() * 1300), 2);
			var status = dayOffset >= -3 ? (rnd.NextDouble() < 0.5 ? "In Transit" : "Pending") : "Paid";

			results.Add(new PayoutItem { Id = idCounter--, Platform = "Manual", Amount = amount, PayoutDate = date, Status = status });

			dayOffset -= 6 + rnd.Next(3);
		}

		return results;
	}

	#endregion
}
