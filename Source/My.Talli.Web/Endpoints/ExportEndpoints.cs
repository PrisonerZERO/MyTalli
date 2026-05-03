namespace My.Talli.Web.Endpoints;

using Domain.Commands.Export;
using System.Text;
using Web.Services.Export;

/// <summary>Endpoint</summary>
public static class ExportEndpoints
{
	#region <Endpoints>

	public static void MapExportEndpoints(this IEndpointRouteBuilder app)
	{
		app.MapGet("/api/export/revenue.csv", DownloadRevenueCsv).RequireAuthorization();
		app.MapGet("/api/export/expenses.csv", DownloadExpensesCsv).RequireAuthorization();
		app.MapGet("/api/export/payouts.csv", DownloadPayoutsCsv).RequireAuthorization();
	}


	#endregion

	#region <Methods>

	private static async Task<IResult> DownloadRevenueCsv(HttpContext context, GetExportDataCommand command, CsvExportService csv, DateTime? from, DateTime? to)
	{
		if (!TryResolveContext(context, from, to, out var userId, out var fromUtc, out var toUtc))
			return Results.Unauthorized();

		var rows = await command.GetRevenueAsync(userId, fromUtc, toUtc);
		var content = csv.BuildRevenueCsv(rows);

		return BuildCsvResponse(content, BuildFilename("revenue", fromUtc, toUtc));
	}

	private static async Task<IResult> DownloadExpensesCsv(HttpContext context, GetExportDataCommand command, CsvExportService csv, DateTime? from, DateTime? to)
	{
		if (!TryResolveContext(context, from, to, out var userId, out var fromUtc, out var toUtc))
			return Results.Unauthorized();

		var rows = await command.GetExpensesAsync(userId, fromUtc, toUtc);
		var content = csv.BuildExpenseCsv(rows);

		return BuildCsvResponse(content, BuildFilename("expenses", fromUtc, toUtc));
	}

	private static async Task<IResult> DownloadPayoutsCsv(HttpContext context, GetExportDataCommand command, CsvExportService csv, DateTime? from, DateTime? to)
	{
		if (!TryResolveContext(context, from, to, out var userId, out var fromUtc, out var toUtc))
			return Results.Unauthorized();

		var rows = await command.GetPayoutsAsync(userId, fromUtc, toUtc);
		var content = csv.BuildPayoutCsv(rows);

		return BuildCsvResponse(content, BuildFilename("payouts", fromUtc, toUtc));
	}

	private static bool TryResolveContext(HttpContext context, DateTime? from, DateTime? to, out long userId, out DateTime fromUtc, out DateTime toUtc)
	{
		userId = 0;
		fromUtc = default;
		toUtc = default;

		var raw = context.User.FindFirst("UserId")?.Value;
		if (!long.TryParse(raw, out userId))
			return false;

		// Default to last 90 days when caller omits filters
		toUtc = (to ?? DateTime.UtcNow).ToUniversalTime();
		fromUtc = (from ?? toUtc.AddDays(-90)).ToUniversalTime();
		return true;
	}

	private static IResult BuildCsvResponse(string content, string fileName)
	{
		// Prefix with UTF-8 BOM so Excel opens it correctly with non-ASCII (currency symbols, etc.)
		var bytes = new List<byte>(Encoding.UTF8.GetPreamble());
		bytes.AddRange(Encoding.UTF8.GetBytes(content));

		return Results.File(bytes.ToArray(), "text/csv; charset=utf-8", fileName);
	}

	private static string BuildFilename(string kind, DateTime from, DateTime to)
		=> $"mytalli-{kind}-{from:yyyyMMdd}-to-{to:yyyyMMdd}.csv";


	#endregion
}
