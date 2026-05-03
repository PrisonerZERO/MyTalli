namespace My.Talli.UnitTesting.Services.Export;

using Domain.Models;
using My.Talli.Web.Services.Export;

/// <summary>Tests</summary>
public class CsvExportServiceTests
{
    #region <Methods>

    [Fact]
    public void BuildRevenueCsv_NoRows_ReturnsHeaderOnly()
    {
        var svc = new CsvExportService();

        var csv = svc.BuildRevenueCsv([]);

        Assert.Equal("Date,Platform,Shop,Description,Currency,Gross,Fee,Net,Refunded,Disputed,PlatformTransactionId\r\n", csv);
    }

    [Fact]
    public void BuildRevenueCsv_SimpleRow_FormatsAllColumns()
    {
        var svc = new CsvExportService();
        var row = new ExportRevenueRow
        {
            Currency = "USD",
            Description = "Order #42",
            FeeAmount = 0.50m,
            GrossAmount = 25.00m,
            IsDisputed = false,
            IsRefunded = false,
            NetAmount = 24.50m,
            Platform = "Etsy",
            PlatformTransactionId = "txn-1",
            ShopName = "MyTalliTestShop",
            TransactionDate = new DateTime(2026, 5, 3, 10, 15, 30, DateTimeKind.Utc)
        };

        var csv = svc.BuildRevenueCsv([row]);
        var lines = csv.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(2, lines.Length);
        Assert.Equal("2026-05-03 10:15:30,Etsy,MyTalliTestShop,Order #42,USD,25.00,0.50,24.50,false,false,txn-1", lines[1]);
    }

    [Fact]
    public void BuildRevenueCsv_DescriptionWithComma_GetsQuoted()
    {
        var svc = new CsvExportService();
        var row = BuildRevenueRow(description: "Two items: hat, scarf");

        var csv = svc.BuildRevenueCsv([row]);

        Assert.Contains("\"Two items: hat, scarf\"", csv);
    }

    [Fact]
    public void BuildRevenueCsv_DescriptionWithQuote_GetsEscapedAndQuoted()
    {
        var svc = new CsvExportService();
        var row = BuildRevenueRow(description: "Says \"hello\"");

        var csv = svc.BuildRevenueCsv([row]);

        Assert.Contains("\"Says \"\"hello\"\"\"", csv);
    }

    [Fact]
    public void BuildRevenueCsv_DescriptionWithNewline_GetsQuoted()
    {
        var svc = new CsvExportService();
        var row = BuildRevenueRow(description: "Line1\nLine2");

        var csv = svc.BuildRevenueCsv([row]);

        Assert.Contains("\"Line1\nLine2\"", csv);
    }

    [Fact]
    public void BuildRevenueCsv_RefundedAndDisputedFlags_RenderAsTrue()
    {
        var svc = new CsvExportService();
        var row = BuildRevenueRow();
        row.IsRefunded = true;
        row.IsDisputed = true;

        var csv = svc.BuildRevenueCsv([row]);
        var dataLine = csv.Split("\r\n")[1];

        Assert.Contains(",true,true,", dataLine);
    }

    [Fact]
    public void BuildRevenueCsv_NullPlatformTransactionId_RendersEmpty()
    {
        var svc = new CsvExportService();
        var row = BuildRevenueRow();
        row.PlatformTransactionId = null;

        var csv = svc.BuildRevenueCsv([row]);

        Assert.EndsWith(",\r\n", csv); // last column empty
    }

    [Fact]
    public void BuildRevenueCsv_DecimalAmounts_UseInvariantCultureWithTwoDecimals()
    {
        var svc = new CsvExportService();
        var row = BuildRevenueRow();
        row.GrossAmount = 1234.5m;
        row.FeeAmount = 12.345m;
        row.NetAmount = 1222.155m;

        var csv = svc.BuildRevenueCsv([row]);

        Assert.Contains("1234.50", csv);
        Assert.Contains("12.35", csv); // rounds to 2 decimals (banker's rounding via "0.00")
        Assert.Contains("1222.16", csv); // rounds 1222.155 -> 1222.16 (away-from-zero rounding default)
    }

    [Fact]
    public void BuildExpenseCsv_RowsRenderInOrder()
    {
        var svc = new CsvExportService();
        var rows = new[]
        {
            new ExportExpenseRow { Amount = 5.00m, Category = "Listing Fee", Currency = "USD", Description = "Listing fee", ExpenseDate = new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc), Platform = "Etsy", PlatformTransactionId = "ledger-1", ShopName = "Shop A" },
            new ExportExpenseRow { Amount = 0.20m, Category = "Processing Fee", Currency = "USD", Description = "Card processing", ExpenseDate = new DateTime(2026, 5, 2, 12, 0, 0, DateTimeKind.Utc), Platform = "Etsy", PlatformTransactionId = null, ShopName = "Shop A" }
        };

        var csv = svc.BuildExpenseCsv(rows);
        var lines = csv.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(3, lines.Length); // header + 2
        Assert.StartsWith("Date,Platform,Shop,Category", lines[0]);
        Assert.Contains("Listing fee", lines[1]);
        Assert.Contains("Card processing", lines[2]);
    }

    [Fact]
    public void BuildPayoutCsv_NullExpectedArrival_RendersEmpty()
    {
        var svc = new CsvExportService();
        var row = new ExportPayoutRow
        {
            Amount = 100.00m,
            Currency = "USD",
            ExpectedArrivalDate = null,
            PayoutDate = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc),
            Platform = "Stripe",
            PlatformPayoutId = "po_test_1",
            ShopName = "MyShop",
            Status = "Paid"
        };

        var csv = svc.BuildPayoutCsv([row]);
        var dataLine = csv.Split("\r\n")[1];
        var cells = dataLine.Split(',');

        // ExpectedArrivalDate is the 7th column (index 6); it should be empty
        Assert.Equal(string.Empty, cells[6]);
    }

    [Fact]
    public void BuildAllCsv_LinesEndWithCRLF_PerRfc4180()
    {
        var svc = new CsvExportService();

        Assert.EndsWith("\r\n", svc.BuildRevenueCsv([]));
        Assert.EndsWith("\r\n", svc.BuildExpenseCsv([]));
        Assert.EndsWith("\r\n", svc.BuildPayoutCsv([]));
    }

    private static ExportRevenueRow BuildRevenueRow(string description = "Order")
    {
        return new ExportRevenueRow
        {
            Currency = "USD",
            Description = description,
            FeeAmount = 0m,
            GrossAmount = 10m,
            NetAmount = 10m,
            Platform = "Etsy",
            PlatformTransactionId = "txn-1",
            ShopName = "Shop",
            TransactionDate = new DateTime(2026, 5, 3, 10, 0, 0, DateTimeKind.Utc)
        };
    }

    #endregion
}
