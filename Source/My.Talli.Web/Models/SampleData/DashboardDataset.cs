namespace My.Talli.Web.Models;

/// <summary>Sample Data</summary>
public static class DashboardDataset
{
	#region <Methods>

	public static string GetChartCurrentLinePath() =>
		"M40,155 L67,148 L94,152 L121,140 L148,145 L175,130 L202,138 L229,125 L256,130 L283,118 L310,124 L337,108 L364,100 L391,105 L418,92 L445,98 L472,85 L499,78 L526,82 L553,70 L580,75 L607,62 L634,68 L661,55 L688,50 L715,45 L742,52 L769,38 L796,32";

	public static string GetChartPreviousLinePath() =>
		"M40,140 L67,135 L94,142 L121,130 L148,138 L175,125 L202,132 L229,120 L256,128 L283,115 L310,122 L337,110 L364,118 L391,105 L418,112 L445,100 L472,108 L499,95 L526,102 L553,90 L580,97 L607,85 L634,92 L661,80 L688,88 L715,75 L742,82 L769,70 L796,78";

	public static List<PlatformBreakdown> GetPlatforms() =>
	[
		new PlatformBreakdown("Stripe", "#635bff", 2340m, 55),
		new PlatformBreakdown("Etsy", "#f56400", 1128m, 27),
		new PlatformBreakdown("Gumroad", "#ff90e8", 750m, 18)
	];

	public static List<Transaction> GetRecentTransactions() =>
	[
		new Transaction("Etsy", "#f56400", "Handmade Candle Set (x2)", "Mar 1", 68.00m),
		new Transaction("Stripe", "#635bff", "Logo Design — Freelance", "Feb 28", 450.00m),
		new Transaction("Gumroad", "#ff90e8", "Procreate Brush Pack", "Feb 27", 12.00m),
		new Transaction("Stripe", "#635bff", "Web Dev Retainer — Feb", "Feb 26", 800.00m),
		new Transaction("Etsy", "#f56400", "Ceramic Mug — Speckled", "Feb 25", 34.00m)
	];

	#endregion
}

public record PlatformBreakdown(string Name, string Color, decimal Amount, int Percentage);

public record Transaction(string PlatformName, string PlatformColor, string Description, string Date, decimal Amount, DateTime SortDate = default);
