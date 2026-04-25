namespace My.Talli.UnitTesting.Services.Platforms;

using Domain.Components;
using Domain.Models;
using Microsoft.Extensions.Logging;
using My.Talli.UnitTesting.Infrastructure.Builders;

/// <summary>Tests</summary>
public class EtsySyncServiceTests
{
    #region <Methods>

    [Fact]
    public async Task SyncShop_NoReceiptsOrLedger_InsertsNothing()
    {
        var builder = new EtsySyncBuilder();
        var shop = await InsertShopAsync(builder);

        var result = await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        Assert.Equal(0, result.NewRevenueRowCount);
        Assert.Equal(0, result.NewExpenseRowCount);
        Assert.Equal(0, result.NewPayoutRowCount);
        Assert.Empty(await builder.RevenueAdapter.GetAllAsync());
    }

    [Fact]
    public async Task SyncShop_PaidReceiptWithTransaction_InsertsRevenue()
    {
        var builder = new EtsySyncBuilder();
        var shop = await InsertShopAsync(builder);
        builder.ApiClient.ReceiptResponses.Enqueue(BuildReceiptsPage([BuildReceipt(transactionId: 555, gross: 25m)]));

        var result = await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        Assert.Equal(1, result.NewRevenueRowCount);
        var revenue = Assert.Single(await builder.RevenueAdapter.GetAllAsync());
        Assert.Equal("555", revenue.PlatformTransactionId);
        Assert.Equal(25m, revenue.GrossAmount);
    }

    [Fact]
    public async Task SyncShop_UnpaidReceipt_Skipped()
    {
        var builder = new EtsySyncBuilder();
        var shop = await InsertShopAsync(builder);
        builder.ApiClient.ReceiptResponses.Enqueue(BuildReceiptsPage([BuildReceipt(transactionId: 555, isPaid: false)]));

        var result = await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        Assert.Equal(0, result.NewRevenueRowCount);
    }

    [Fact]
    public async Task SyncShop_FullPage_LoopsToNextPage()
    {
        var builder = new EtsySyncBuilder();
        var shop = await InsertShopAsync(builder);

        var firstPage = BuildReceiptsPage(Enumerable.Range(1, 100).Select(i => BuildReceipt(transactionId: i)).ToArray());
        var secondPage = BuildReceiptsPage([BuildReceipt(transactionId: 101)]);
        builder.ApiClient.ReceiptResponses.Enqueue(firstPage);
        builder.ApiClient.ReceiptResponses.Enqueue(secondPage);

        var result = await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        Assert.Equal(101, result.NewRevenueRowCount);
        Assert.Equal(2, result.PagesFetched);
        Assert.Equal(2, builder.ApiClient.ReceiptCalls.Count);
        Assert.Equal(0, builder.ApiClient.ReceiptCalls[0].Offset);
        Assert.Equal(100, builder.ApiClient.ReceiptCalls[1].Offset);
    }

    [Fact]
    public async Task SyncShop_ShortPage_StopsPagination()
    {
        var builder = new EtsySyncBuilder();
        var shop = await InsertShopAsync(builder);
        builder.ApiClient.ReceiptResponses.Enqueue(BuildReceiptsPage([BuildReceipt(transactionId: 1)]));

        await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        Assert.Single(builder.ApiClient.ReceiptCalls);
    }

    [Fact]
    public async Task SyncShop_LedgerSaleAndRefund_SkippedSilently()
    {
        var builder = new EtsySyncBuilder();
        var shop = await InsertShopAsync(builder);
        builder.ApiClient.LedgerResponses.Enqueue(BuildLedgerPage([
            BuildLedger(entryId: 1, type: "Sale"),
            BuildLedger(entryId: 2, type: "Refund")
        ]));

        var result = await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        Assert.Equal(0, result.NewExpenseRowCount);
        Assert.Equal(0, result.NewPayoutRowCount);
        Assert.DoesNotContain(builder.Logger.Entries, e => e.Level == LogLevel.Warning);
    }

    [Fact]
    public async Task SyncShop_LedgerPayoutTypes_InsertedAsPayouts()
    {
        var builder = new EtsySyncBuilder();
        var shop = await InsertShopAsync(builder);
        builder.ApiClient.LedgerResponses.Enqueue(BuildLedgerPage([
            BuildLedger(entryId: 10, type: "Payment", amount: -100m),
            BuildLedger(entryId: 11, type: "Disbursement", amount: -50m),
            BuildLedger(entryId: 12, type: "Withdrawal", amount: -25m)
        ]));

        var result = await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        Assert.Equal(3, result.NewPayoutRowCount);
        var payouts = (await builder.PayoutAdapter.GetAllAsync()).ToDictionary(p => p.PlatformPayoutId);
        Assert.Equal(100m, payouts["10"].Amount);
        Assert.Equal(50m, payouts["11"].Amount);
        Assert.Equal(25m, payouts["12"].Amount);
    }

    [Fact]
    public async Task SyncShop_LedgerFeeTypes_InsertedAsExpensesWithMappedCategory()
    {
        var builder = new EtsySyncBuilder();
        var shop = await InsertShopAsync(builder);
        builder.ApiClient.LedgerResponses.Enqueue(BuildLedgerPage([
            BuildLedger(entryId: 1, type: "Listing Fee", amount: -0.20m),
            BuildLedger(entryId: 2, type: "Promoted Listing Fee", amount: -1.50m),
            BuildLedger(entryId: 3, type: "Marketing Fee", amount: -2m),
            BuildLedger(entryId: 4, type: "Subscription Fee", amount: -10m),
            BuildLedger(entryId: 5, type: "Transaction Fee", amount: -0.50m),
            BuildLedger(entryId: 6, type: "Processing Fee", amount: -0.25m),
            BuildLedger(entryId: 7, type: "Postage Label", amount: -3m),
            BuildLedger(entryId: 8, type: "Shipping Label", amount: -4m),
            BuildLedger(entryId: 9, type: "Tax", amount: -1m),
            BuildLedger(entryId: 10, type: "VAT", amount: -2m)
        ]));

        var result = await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        Assert.Equal(10, result.NewExpenseRowCount);
        var expenses = (await builder.ExpenseAdapter.GetAllAsync()).ToDictionary(e => e.PlatformTransactionId!);
        Assert.Equal("Listing Fee", expenses["1"].Category);
        Assert.Equal("Ad Fee", expenses["2"].Category);
        Assert.Equal("Ad Fee", expenses["3"].Category);
        Assert.Equal("Subscription Fee", expenses["4"].Category);
        Assert.Equal("Processing Fee", expenses["5"].Category);
        Assert.Equal("Processing Fee", expenses["6"].Category);
        Assert.Equal("Shipping Label", expenses["7"].Category);
        Assert.Equal("Shipping Label", expenses["8"].Category);
        Assert.Equal("Other", expenses["9"].Category);
        Assert.Equal("Other", expenses["10"].Category);
    }

    [Fact]
    public async Task SyncShop_LedgerExpenseAmount_StoredAsAbsoluteValue()
    {
        var builder = new EtsySyncBuilder();
        var shop = await InsertShopAsync(builder);
        builder.ApiClient.LedgerResponses.Enqueue(BuildLedgerPage([
            BuildLedger(entryId: 1, type: "Listing Fee", amount: -0.20m)
        ]));

        await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        var expense = Assert.Single(await builder.ExpenseAdapter.GetAllAsync());
        Assert.Equal(0.20m, expense.Amount);
    }

    [Fact]
    public async Task SyncShop_UnknownLedgerType_LogsWarningAndSkips()
    {
        var builder = new EtsySyncBuilder();
        var shop = await InsertShopAsync(builder);
        builder.ApiClient.LedgerResponses.Enqueue(BuildLedgerPage([
            BuildLedger(entryId: 99, type: "Currency Conversion Fee")
        ]));

        var result = await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        Assert.Equal(0, result.NewExpenseRowCount);
        Assert.Equal(0, result.NewPayoutRowCount);
        var warning = Assert.Single(builder.Logger.Entries, e => e.Level == LogLevel.Warning);
        Assert.Contains("Currency Conversion Fee", warning.Message);
    }

    [Fact]
    public async Task SyncShop_NullLedgerType_LogsWarningAndSkips()
    {
        var builder = new EtsySyncBuilder();
        var shop = await InsertShopAsync(builder);
        builder.ApiClient.LedgerResponses.Enqueue(BuildLedgerPage([
            BuildLedger(entryId: 99, type: null)
        ]));

        await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        var warning = Assert.Single(builder.Logger.Entries, e => e.Level == LogLevel.Warning);
        Assert.Contains("(null)", warning.Message);
    }

    [Fact]
    public async Task SyncShop_TokenExpired_RefreshesInlineBeforeFetching()
    {
        var builder = new EtsySyncBuilder();
        var shop = await InsertShopAsync(builder, tokenExpiry: DateTime.UtcNow.AddMinutes(-10), refreshToken: "old-refresh");
        builder.ApiClient.RefreshResponse = new EtsyTokenResponse
        {
            AccessToken = "new-access",
            RefreshToken = "new-refresh",
            ExpiresIn = 3600
        };

        await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        Assert.Single(builder.ApiClient.RefreshCalls);
        Assert.Equal("old-refresh", builder.ApiClient.RefreshCalls[0]);

        var stored = await builder.ShopConnectionAdapter.GetByIdAsync(shop.Id);
        Assert.NotNull(stored);
        Assert.Equal("new-access", stored!.AccessToken);
        Assert.Equal("new-refresh", stored.RefreshToken);
    }

    [Fact]
    public async Task SyncShop_TokenExpiredButNoRefreshToken_Throws()
    {
        var builder = new EtsySyncBuilder();
        var shop = await InsertShopAsync(builder, tokenExpiry: DateTime.UtcNow.AddMinutes(-10), refreshToken: null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => builder.Service.SyncShopAsync(shop, CancellationToken.None));
    }

    [Fact]
    public async Task SyncShop_TokenStillValid_DoesNotCallRefresh()
    {
        var builder = new EtsySyncBuilder();
        var shop = await InsertShopAsync(builder, tokenExpiry: DateTime.UtcNow.AddHours(1));

        await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        Assert.Empty(builder.ApiClient.RefreshCalls);
    }

    [Fact]
    public async Task SyncShop_SecondSyncAfterFirst_DedupsExistingRevenue()
    {
        var builder = new EtsySyncBuilder();
        var shop = await InsertShopAsync(builder);
        builder.ApiClient.ReceiptResponses.Enqueue(BuildReceiptsPage([BuildReceipt(transactionId: 555)]));
        await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        builder.ApiClient.ReceiptResponses.Enqueue(BuildReceiptsPage([BuildReceipt(transactionId: 555)]));
        var second = await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        Assert.Equal(0, second.NewRevenueRowCount);
        Assert.Single(await builder.RevenueAdapter.GetAllAsync());
    }

    private static async Task<ShopConnection> InsertShopAsync(EtsySyncBuilder builder, DateTime? tokenExpiry = null, string? refreshToken = "refresh-token")
    {
        return await builder.ShopConnectionAdapter.InsertAsync(new ShopConnection
        {
            AccessToken = "access-token",
            PlatformAccountId = "etsy-user",
            PlatformConnectionId = 1,
            PlatformShopId = "12345",
            RefreshToken = refreshToken,
            ShopName = "Test Shop",
            Status = "Active",
            TokenExpiryDateTime = tokenExpiry ?? DateTime.UtcNow.AddHours(1),
            UserId = 42
        });
    }

    private static EtsyReceiptsResponse BuildReceiptsPage(IReadOnlyList<EtsyReceipt> receipts)
    {
        return new EtsyReceiptsResponse { Count = receipts.Count, Results = receipts.ToList() };
    }

    private static EtsyReceipt BuildReceipt(long transactionId, decimal gross = 10m, bool isPaid = true)
    {
        var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return new EtsyReceipt
        {
            CreateTimestamp = ts,
            IsPaid = isPaid,
            ReceiptId = transactionId * 10,
            Transactions = new List<EtsyTransaction>
            {
                new()
                {
                    ListingId = transactionId * 100,
                    PaidTimestamp = ts,
                    Price = new EtsyMoney { Amount = (long)(gross * 100), CurrencyCode = "USD", Divisor = 100 },
                    Quantity = 1,
                    Title = $"Item {transactionId}",
                    TransactionId = transactionId
                }
            }
        };
    }

    private static EtsyLedgerEntriesResponse BuildLedgerPage(IReadOnlyList<EtsyLedgerEntry> entries)
    {
        return new EtsyLedgerEntriesResponse { Count = entries.Count, Results = entries.ToList() };
    }

    private static EtsyLedgerEntry BuildLedger(long entryId, string? type, decimal amount = -1m)
    {
        return new EtsyLedgerEntry
        {
            Amount = amount,
            CreateDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Currency = "USD",
            Description = type ?? "Unknown",
            EntryId = entryId,
            LedgerEntryType = type
        };
    }

    #endregion
}
