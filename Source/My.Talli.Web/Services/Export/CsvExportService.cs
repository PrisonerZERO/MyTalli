namespace My.Talli.Web.Services.Export;

using Domain.Models;
using System.Globalization;
using System.Text;

/// <summary>Service</summary>
public class CsvExportService
{
	#region <Constants>

	private const string DateFormat = "yyyy-MM-dd";
	private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
	private const string DecimalFormat = "0.00";
	private static readonly char[] EscapeChars = [',', '"', '\n', '\r'];


	#endregion

	#region <Methods>

	public string BuildRevenueCsv(IEnumerable<ExportRevenueRow> rows)
	{
		var sb = new StringBuilder();
		AppendHeader(sb, "Date", "Platform", "Shop", "Description", "Currency", "Gross", "Fee", "Net", "Refunded", "Disputed", "PlatformTransactionId");

		foreach (var r in rows)
		{
			AppendRow(sb,
				r.TransactionDate.ToString(DateTimeFormat, CultureInfo.InvariantCulture),
				r.Platform,
				r.ShopName,
				r.Description,
				r.Currency,
				r.GrossAmount.ToString(DecimalFormat, CultureInfo.InvariantCulture),
				r.FeeAmount.ToString(DecimalFormat, CultureInfo.InvariantCulture),
				r.NetAmount.ToString(DecimalFormat, CultureInfo.InvariantCulture),
				r.IsRefunded ? "true" : "false",
				r.IsDisputed ? "true" : "false",
				r.PlatformTransactionId ?? string.Empty);
		}

		return sb.ToString();
	}

	public string BuildExpenseCsv(IEnumerable<ExportExpenseRow> rows)
	{
		var sb = new StringBuilder();
		AppendHeader(sb, "Date", "Platform", "Shop", "Category", "Description", "Currency", "Amount", "PlatformTransactionId");

		foreach (var e in rows)
		{
			AppendRow(sb,
				e.ExpenseDate.ToString(DateTimeFormat, CultureInfo.InvariantCulture),
				e.Platform,
				e.ShopName,
				e.Category,
				e.Description,
				e.Currency,
				e.Amount.ToString(DecimalFormat, CultureInfo.InvariantCulture),
				e.PlatformTransactionId ?? string.Empty);
		}

		return sb.ToString();
	}

	public string BuildPayoutCsv(IEnumerable<ExportPayoutRow> rows)
	{
		var sb = new StringBuilder();
		AppendHeader(sb, "Date", "Platform", "Shop", "Currency", "Amount", "Status", "ExpectedArrivalDate", "PlatformPayoutId");

		foreach (var p in rows)
		{
			AppendRow(sb,
				p.PayoutDate.ToString(DateTimeFormat, CultureInfo.InvariantCulture),
				p.Platform,
				p.ShopName,
				p.Currency,
				p.Amount.ToString(DecimalFormat, CultureInfo.InvariantCulture),
				p.Status,
				p.ExpectedArrivalDate?.ToString(DateFormat, CultureInfo.InvariantCulture) ?? string.Empty,
				p.PlatformPayoutId ?? string.Empty);
		}

		return sb.ToString();
	}

	private static void AppendHeader(StringBuilder sb, params string[] columns) => AppendRow(sb, columns);

	private static void AppendRow(StringBuilder sb, params string[] cells)
	{
		for (var i = 0; i < cells.Length; i++)
		{
			if (i > 0) sb.Append(',');
			sb.Append(Escape(cells[i]));
		}
		sb.Append("\r\n"); // RFC 4180 — CRLF line endings
	}

	/// <summary>RFC 4180 — wrap in double quotes if value contains comma, quote, or newline; double up embedded quotes.</summary>
	private static string Escape(string value)
	{
		if (string.IsNullOrEmpty(value))
			return string.Empty;

		if (value.IndexOfAny(EscapeChars) < 0)
			return value;

		return $"\"{value.Replace("\"", "\"\"")}\"";
	}


	#endregion
}
