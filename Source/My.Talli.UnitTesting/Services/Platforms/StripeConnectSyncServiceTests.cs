namespace My.Talli.UnitTesting.Services.Platforms;

using Domain.Models;
using My.Talli.UnitTesting.Infrastructure.Builders;
using Stripe;

using STRIPE = Stripe;

/// <summary>Tests</summary>
public class StripeConnectSyncServiceTests
{
    #region <Methods>

    [Fact]
    public async Task SyncShop_NoCharges_NoPayouts_InsertsNothing()
    {
        var builder = new StripeSyncBuilder();
        var shop = await InsertShopAsync(builder);

        var result = await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        Assert.Equal(0, result.NewRevenueRowCount);
        Assert.Equal(0, result.NewPayoutRowCount);
        Assert.Empty(await builder.RevenueAdapter.GetAllAsync());
        Assert.Empty(await builder.PayoutAdapter.GetAllAsync());
    }

    [Fact]
    public async Task SyncShop_SinglePageOfCharges_InsertsAll()
    {
        var builder = new StripeSyncBuilder();
        var shop = await InsertShopAsync(builder);
        builder.ApiClient.ChargeResponses.Enqueue(BuildChargesPage([
            BuildCharge(id: "ch_1", balanceTxnId: "txn_1", amount: 1500, fee: 50, net: 1450),
            BuildCharge(id: "ch_2", balanceTxnId: "txn_2", amount: 2500, fee: 75, net: 2425),
        ]));

        var result = await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        Assert.Equal(2, result.NewRevenueRowCount);
        Assert.Equal(2, (await builder.RevenueAdapter.GetAllAsync()).Count());
        Assert.Equal(2, (await builder.RevenueStripeAdapter.GetAllAsync()).Count());
    }

    [Fact]
    public async Task SyncShop_AmountInCentsConvertedToDecimalDollars()
    {
        var builder = new StripeSyncBuilder();
        var shop = await InsertShopAsync(builder);
        builder.ApiClient.ChargeResponses.Enqueue(BuildChargesPage([
            BuildCharge(id: "ch_1", balanceTxnId: "txn_1", amount: 1234, fee: 60, net: 1174),
        ]));

        await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        var revenue = Assert.Single(await builder.RevenueAdapter.GetAllAsync());
        Assert.Equal(12.34m, revenue.GrossAmount);
        Assert.Equal(0.60m, revenue.FeeAmount);
        Assert.Equal(11.74m, revenue.NetAmount);
    }

    [Fact]
    public async Task SyncShop_RefundedAndDisputedFlags_StoredOnRevenue()
    {
        var builder = new StripeSyncBuilder();
        var shop = await InsertShopAsync(builder);
        var charge = BuildCharge(id: "ch_1", balanceTxnId: "txn_1");
        charge.Refunded = true;
        charge.Disputed = true;
        builder.ApiClient.ChargeResponses.Enqueue(BuildChargesPage([charge]));

        await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        var revenue = Assert.Single(await builder.RevenueAdapter.GetAllAsync());
        Assert.True(revenue.IsRefunded);
        Assert.True(revenue.IsDisputed);
    }

    [Fact]
    public async Task SyncShop_BalanceTransactionIdUsedAsDedupKey()
    {
        var builder = new StripeSyncBuilder();
        var shop = await InsertShopAsync(builder);
        builder.ApiClient.ChargeResponses.Enqueue(BuildChargesPage([
            BuildCharge(id: "ch_1", balanceTxnId: "txn_1"),
        ]));

        await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        var detail = Assert.Single(await builder.RevenueStripeAdapter.GetAllAsync());
        Assert.Equal("txn_1", detail.BalanceTransactionId);
        var revenue = Assert.Single(await builder.RevenueAdapter.GetAllAsync());
        Assert.Equal("txn_1", revenue.PlatformTransactionId);
    }

    [Fact]
    public async Task SyncShop_HasMore_FollowsToSecondPage()
    {
        var builder = new StripeSyncBuilder();
        var shop = await InsertShopAsync(builder);

        var firstPage = BuildChargesPage([BuildCharge(id: "ch_1", balanceTxnId: "txn_1")], hasMore: true);
        var secondPage = BuildChargesPage([BuildCharge(id: "ch_2", balanceTxnId: "txn_2")]);
        builder.ApiClient.ChargeResponses.Enqueue(firstPage);
        builder.ApiClient.ChargeResponses.Enqueue(secondPage);

        var result = await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        Assert.Equal(2, result.NewRevenueRowCount);
        Assert.Equal(2, builder.ApiClient.ChargeCalls.Count);
        Assert.Null(builder.ApiClient.ChargeCalls[0].StartingAfter);
        Assert.Equal("ch_1", builder.ApiClient.ChargeCalls[1].StartingAfter);
    }

    [Fact]
    public async Task SyncShop_DedupAcrossSyncs_OnlyInsertsNew()
    {
        var builder = new StripeSyncBuilder();
        var shop = await InsertShopAsync(builder);
        builder.ApiClient.ChargeResponses.Enqueue(BuildChargesPage([BuildCharge(id: "ch_1", balanceTxnId: "txn_1")]));
        await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        builder.ApiClient.ChargeResponses.Enqueue(BuildChargesPage([
            BuildCharge(id: "ch_1", balanceTxnId: "txn_1"),
            BuildCharge(id: "ch_2", balanceTxnId: "txn_2"),
        ]));
        var result = await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        Assert.Equal(1, result.NewRevenueRowCount);
        Assert.Equal(2, (await builder.RevenueAdapter.GetAllAsync()).Count());
    }

    [Fact]
    public async Task SyncShop_NoLastSync_CreatedAfterIsBackfillFloor()
    {
        var builder = new StripeSyncBuilder();
        var shop = await InsertShopAsync(builder);
        shop.LastSyncDateTime = null;

        var before = DateTime.UtcNow.AddDays(-90).AddSeconds(-2);
        await builder.Service.SyncShopAsync(shop, CancellationToken.None);
        var after = DateTime.UtcNow.AddDays(-90).AddSeconds(2);

        var afterDate = builder.ApiClient.ChargeCalls[0].CreatedAfter;
        Assert.NotNull(afterDate);
        Assert.InRange(afterDate.Value, before, after);
    }

    [Fact]
    public async Task SyncShop_LastSyncSet_CreatedAfterUsesLastSync()
    {
        var builder = new StripeSyncBuilder();
        var lastSync = new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc);
        var shop = await InsertShopAsync(builder);
        shop.LastSyncDateTime = lastSync;

        await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        Assert.Equal(lastSync, builder.ApiClient.ChargeCalls[0].CreatedAfter);
    }

    [Fact]
    public async Task SyncShop_PayoutPage_InsertsAll()
    {
        var builder = new StripeSyncBuilder();
        var shop = await InsertShopAsync(builder);
        builder.ApiClient.PayoutResponses.Enqueue(BuildPayoutsPage([
            BuildPayout(id: "po_1", amount: 25000),
            BuildPayout(id: "po_2", amount: 50000),
        ]));

        var result = await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        Assert.Equal(2, result.NewPayoutRowCount);
        Assert.Equal(2, (await builder.PayoutAdapter.GetAllAsync()).Count());
        Assert.Equal(2, (await builder.PayoutStripeAdapter.GetAllAsync()).Count());
    }

    [Fact]
    public async Task SyncShop_PayoutAmountConvertedToDecimal()
    {
        var builder = new StripeSyncBuilder();
        var shop = await InsertShopAsync(builder);
        builder.ApiClient.PayoutResponses.Enqueue(BuildPayoutsPage([BuildPayout(id: "po_1", amount: 12345)]));

        await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        var payout = Assert.Single(await builder.PayoutAdapter.GetAllAsync());
        Assert.Equal(123.45m, payout.Amount);
    }

    [Fact]
    public async Task SyncShop_PayoutHasMore_FollowsToSecondPage()
    {
        var builder = new StripeSyncBuilder();
        var shop = await InsertShopAsync(builder);

        var firstPage = BuildPayoutsPage([BuildPayout(id: "po_1")], hasMore: true);
        var secondPage = BuildPayoutsPage([BuildPayout(id: "po_2")]);
        builder.ApiClient.PayoutResponses.Enqueue(firstPage);
        builder.ApiClient.PayoutResponses.Enqueue(secondPage);

        var result = await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        Assert.Equal(2, result.NewPayoutRowCount);
        Assert.Equal(2, builder.ApiClient.PayoutCalls.Count);
        Assert.Equal("po_1", builder.ApiClient.PayoutCalls[1].StartingAfter);
    }

    [Fact]
    public async Task SyncShop_StripeAccountIdPassedToApiCalls()
    {
        var builder = new StripeSyncBuilder();
        var shop = await InsertShopAsync(builder, accountId: "acct_xyz");

        await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        Assert.Equal("acct_xyz", builder.ApiClient.ChargeCalls[0].AccountId);
        Assert.Equal("acct_xyz", builder.ApiClient.PayoutCalls[0].AccountId);
    }

    [Fact]
    public void PlatformPropertyReturnsStripe()
    {
        var builder = new StripeSyncBuilder();

        Assert.Equal("Stripe", builder.Service.Platform);
    }

    private static async Task<ShopConnection> InsertShopAsync(StripeSyncBuilder builder, string accountId = "acct_test_001")
    {
        return await builder.ShopConnectionAdapter.InsertAsync(new ShopConnection
        {
            AccessToken = string.Empty,
            PlatformAccountId = accountId,
            PlatformConnectionId = 1,
            PlatformShopId = accountId,
            RefreshToken = null,
            ShopName = "Test Stripe",
            Status = "Active",
            TokenExpiryDateTime = null,
            UserId = 42
        });
    }

    private static StripeList<Charge> BuildChargesPage(IReadOnlyList<Charge> charges, bool hasMore = false)
    {
        return new StripeList<Charge>
        {
            Data = charges.ToList(),
            HasMore = hasMore
        };
    }

    private static Charge BuildCharge(string id, string balanceTxnId, long amount = 1000, long fee = 30, long net = 970)
    {
        return new Charge
        {
            Amount = amount,
            BalanceTransaction = new BalanceTransaction
            {
                ExchangeRate = null,
                Fee = fee,
                Id = balanceTxnId,
                Net = net
            },
            BalanceTransactionId = balanceTxnId,
            Created = new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc),
            Currency = "usd",
            Description = "Test charge",
            Disputed = false,
            Id = id,
            PaymentMethodDetails = new ChargePaymentMethodDetails { Type = "card" },
            Refunded = false
        };
    }

    private static StripeList<STRIPE.Payout> BuildPayoutsPage(IReadOnlyList<STRIPE.Payout> payouts, bool hasMore = false)
    {
        return new StripeList<STRIPE.Payout>
        {
            Data = payouts.ToList(),
            HasMore = hasMore
        };
    }

    private static STRIPE.Payout BuildPayout(string id, long amount = 10000)
    {
        return new STRIPE.Payout
        {
            Amount = amount,
            ArrivalDate = new DateTime(2026, 5, 5, 12, 0, 0, DateTimeKind.Utc),
            Created = new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc),
            Currency = "usd",
            Id = id,
            Method = "standard",
            Status = "in_transit"
        };
    }

    #endregion
}
