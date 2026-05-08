namespace My.Talli.Web.Models;

/// <summary>Sample Data</summary>
public static class ManualEntryDataset
{
	#region <Methods>

	public static List<ManualEntryItem> GetEntries()
	{
		var today = DateTime.Today;
		var rnd = new Random(45);
		var freelance = new[] { ("Logo design for startup", 850m, "Branding package for TechNova"), ("Icon set commission", 320m, "40 custom icons for SaaS dashboard"), ("Wedding invitation design", 400m, "Rush order, premium rate"), ("Brochure layout", 275m, (string?)null), ("Business card design", 150m, (string?)null), ("Social media graphics pack", 220m, "12 templates"), ("Book cover illustration", 540m, (string?)null), ("Menu redesign — café", 360m, (string?)null) };
		var consulting = new[] { ("Monthly consulting retainer", 1200m, (string?)null), ("Brand strategy session", 500m, (string?)null), ("SEO audit report", 750m, (string?)null), ("Marketing strategy call", 250m, (string?)null), ("Content audit", 420m, (string?)null), ("Pricing model review", 600m, (string?)null) };
		var service = new[] { ("Workshop facilitation", 600m, 2, (string?)null), ("Photography editing batch", 280m, 14, (string?)null), ("Voiceover recording", 180m, 1, (string?)null), ("Video editing — promo", 350m, 1, (string?)null), ("Headshot session", 220m, 1, (string?)null) };
		var sale = new[] { ("Handmade candles (market)", 185m, 37, (string?)null), ("Craft fair earnings", 221m, 12, (string?)null), ("Pop-up shop sales", 412m, 23, (string?)null), ("Farmers market booth", 168m, 19, (string?)null), ("Holiday market sales", 624m, 42, (string?)null) };
		var digital = new[] { ("UI template pack", 45m, 3, "Sold on personal site"), ("Notion template bundle", 29m, 1, (string?)null), ("Procreate brush set", 18m, 1, (string?)null), ("Lightroom presets", 24m, 2, (string?)null), ("Wedding invite template", 35m, 1, (string?)null) };
		var physical = new[] { ("Hand-thrown ceramics", 95m, 4, (string?)null), ("Knit scarves (set)", 140m, 2, (string?)null), ("Custom tote bags", 75m, 5, (string?)null), ("Pottery commission", 220m, 1, "Wedding gift") };
		var other = new[] { ("Tip jar — open mic", 42m, (string?)null), ("Affiliate payout", 88m, (string?)null), ("Speaking honorarium", 350m, "20-min keynote at meetup") };
		var results = new List<ManualEntryItem>();
		var idCounter = -1L;

		for (var dayOffset = -400; dayOffset <= -1; dayOffset++)
		{
			var date = today.AddDays(dayOffset);

			var count = rnd.NextDouble() switch
			{
				< 0.55 => 0,
				< 0.80 => 1,
				< 0.95 => 2,
				_ => 3,
			};

			for (var i = 0; i < count; i++)
			{
				var roll = rnd.NextDouble();

				if (roll < 0.20)
				{
					var (desc, gross, notes) = freelance[rnd.Next(freelance.Length)];
					var actualGross = Math.Round(gross * (decimal)(0.85 + rnd.NextDouble() * 0.30), 2);
					results.Add(BuildEntry(idCounter--, "Freelance", desc, actualGross, 1, date, notes));
				}
				else if (roll < 0.35)
				{
					var (desc, gross, notes) = consulting[rnd.Next(consulting.Length)];
					var actualGross = Math.Round(gross * (decimal)(0.85 + rnd.NextDouble() * 0.30), 2);
					results.Add(BuildEntry(idCounter--, "Consulting", desc, actualGross, 1, date, notes));
				}
				else if (roll < 0.50)
				{
					var (desc, gross, qty, notes) = service[rnd.Next(service.Length)];
					var actualGross = Math.Round(gross * (decimal)(0.85 + rnd.NextDouble() * 0.30), 2);
					results.Add(BuildEntry(idCounter--, "Service", desc, actualGross, qty, date, notes));
				}
				else if (roll < 0.70)
				{
					var (desc, gross, qty, notes) = sale[rnd.Next(sale.Length)];
					var actualGross = Math.Round(gross * (decimal)(0.85 + rnd.NextDouble() * 0.30), 2);
					results.Add(BuildEntry(idCounter--, "Sale", desc, actualGross, qty, date, notes, feeRate: 0m));
				}
				else if (roll < 0.85)
				{
					var (desc, gross, qty, notes) = digital[rnd.Next(digital.Length)];
					var actualGross = Math.Round(gross * (decimal)(0.85 + rnd.NextDouble() * 0.30), 2);
					results.Add(BuildEntry(idCounter--, "Digital Product", desc, actualGross, qty, date, notes));
				}
				else if (roll < 0.95)
				{
					var (desc, gross, qty, notes) = physical[rnd.Next(physical.Length)];
					var actualGross = Math.Round(gross * (decimal)(0.85 + rnd.NextDouble() * 0.30), 2);
					results.Add(BuildEntry(idCounter--, "Physical Product", desc, actualGross, qty, date, notes));
				}
				else
				{
					var (desc, gross, notes) = other[rnd.Next(other.Length)];
					var actualGross = Math.Round(gross * (decimal)(0.85 + rnd.NextDouble() * 0.30), 2);
					results.Add(BuildEntry(idCounter--, "Other", desc, actualGross, 1, date, notes));
				}
			}
		}

		return results;
	}

	private static ManualEntryItem BuildEntry(long id, string category, string description, decimal gross, int quantity, DateTime date, string? notes, decimal feeRate = 0.05m)
	{
		var fee = Math.Round(gross * feeRate, 2);

		return new ManualEntryItem
		{
			Id = id,
			Description = description,
			Category = category,
			GrossAmount = gross,
			FeeAmount = fee,
			NetAmount = gross - fee,
			Quantity = quantity,
			Currency = "USD",
			TransactionDate = date,
			Notes = notes,
		};
	}

	#endregion
}
