namespace My.Talli.Web.Models;

/// <summary>Sample Data</summary>
public static class RevenueDataset
{
	#region <Variables>

	private static readonly DailyCache<List<SampleRevenue>> _cache = new(Generate);

	#endregion

	#region <Methods>

	public static List<SampleRevenue> GetSampleRevenues() => _cache.Get();

	private static List<SampleRevenue> Generate(DateTime today)
	{
		var rnd = new Random(42);
		var stripeDescriptions = new[] { "Logo Design — Freelance", "Web Dev Retainer", "Consulting Call (1hr)", "Brand Strategy Session", "Email Setup Project", "Custom Plugin Development", "Site Audit & Recommendations", "Landing Page Build", "Tech Discovery Workshop" };
		var etsyDescriptions = new[] { "Handmade Candle Set (x2)", "Ceramic Mug — Speckled", "Linen Table Runner", "Custom Pet Portrait", "Wood-burned Coaster Set", "Hand-stamped Necklace", "Beeswax Wraps (3-pack)", "Ribbon Wreath — Spring", "Embroidered Tea Towel" };
		var gumroadDescriptions = new[] { "Procreate Brush Pack", "Notion Template Bundle", "SVG Cut File Library", "Lightroom Preset Collection", "Productivity Workbook", "Watercolor Texture Pack", "Figma UI Kit" };
		var results = new List<SampleRevenue>();

		for (var dayOffset = -800; dayOffset <= -1; dayOffset++)
		{
			var date = today.AddDays(dayOffset);

			var count = rnd.NextDouble() switch
			{
				< 0.20 => 0,
				< 0.45 => 1,
				< 0.68 => 2,
				< 0.85 => 3,
				< 0.95 => 4,
				_ => 5,
			};

			for (var i = 0; i < count; i++)
			{
				var roll = rnd.NextDouble();

				if (roll < 0.50)
					results.Add(new SampleRevenue(date, "Stripe", stripeDescriptions[rnd.Next(stripeDescriptions.Length)], Math.Round((decimal)(50 + rnd.NextDouble() * 1150), 2)));
				else if (roll < 0.80)
					results.Add(new SampleRevenue(date, "Etsy", etsyDescriptions[rnd.Next(etsyDescriptions.Length)], Math.Round((decimal)(15 + rnd.NextDouble() * 105), 2)));
				else
					results.Add(new SampleRevenue(date, "Gumroad", gumroadDescriptions[rnd.Next(gumroadDescriptions.Length)], Math.Round((decimal)(5 + rnd.NextDouble() * 45), 2)));
			}
		}

		return results;
	}

	#endregion
}

public record PlatformBreakdown(string Name, string Color, decimal Amount, int Percentage);

public record SampleRevenue(DateTime TransactionDate, string Platform, string Description, decimal NetAmount);

public record Transaction(string PlatformName, string PlatformColor, string Description, string Date, decimal Amount, DateTime SortDate = default);
