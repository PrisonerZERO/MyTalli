namespace My.Talli.UnitTesting.Services.Platforms;

using Domain.Components;
using Domain.Models;
using My.Talli.UnitTesting.Infrastructure.Builders;

/// <summary>Tests</summary>
public class GumroadSyncServiceTests
{
    #region <Methods>

    [Fact]
    public async Task SyncShop_NoSales_InsertsNothing()
    {
        var builder = new GumroadSyncBuilder();
        var shop = await InsertShopAsync(builder);

        var result = await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        Assert.Equal(0, result.NewRevenueRowCount);
        Assert.Empty(await builder.RevenueAdapter.GetAllAsync());
        Assert.Empty(await builder.RevenueGumroadAdapter.GetAllAsync());
    }

    [Fact]
    public async Task SyncShop_SinglePageOfSales_InsertsAll()
    {
        var builder = new GumroadSyncBuilder();
        var shop = await InsertShopAsync(builder);
        builder.ApiClient.SalesResponses.Enqueue(BuildSalesPage([
            BuildSale(id: "s1", priceCents: 1500),
            BuildSale(id: "s2", priceCents: 2500),
        ]));

        var result = await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        Assert.Equal(2, result.NewRevenueRowCount);
        Assert.Equal(2, (await builder.RevenueAdapter.GetAllAsync()).Count());
        Assert.Equal(2, (await builder.RevenueGumroadAdapter.GetAllAsync()).Count());
    }

    [Fact]
    public async Task SyncShop_PriceCentsConvertedToDecimalDollars()
    {
        var builder = new GumroadSyncBuilder();
        var shop = await InsertShopAsync(builder);
        builder.ApiClient.SalesResponses.Enqueue(BuildSalesPage([BuildSale(id: "s1", priceCents: 1234)]));

        await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        var revenue = Assert.Single(await builder.RevenueAdapter.GetAllAsync());
        Assert.Equal(12.34m, revenue.GrossAmount);
        Assert.Equal(12.34m, revenue.NetAmount);
        Assert.Equal(0m, revenue.FeeAmount);
    }

    [Fact]
    public async Task SyncShop_RefundedAndDisputedFlags_StoredOnRevenue()
    {
        var builder = new GumroadSyncBuilder();
        var shop = await InsertShopAsync(builder);
        var sale = BuildSale(id: "s1");
        sale.Refunded = true;
        sale.Disputed = true;
        builder.ApiClient.SalesResponses.Enqueue(BuildSalesPage([sale]));

        await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        var revenue = Assert.Single(await builder.RevenueAdapter.GetAllAsync());
        Assert.True(revenue.IsRefunded);
        Assert.True(revenue.IsDisputed);
    }

    [Fact]
    public async Task SyncShop_LicenseKeyStoredOnRevenueGumroad()
    {
        var builder = new GumroadSyncBuilder();
        var shop = await InsertShopAsync(builder);
        var sale = BuildSale(id: "s1");
        sale.LicenseKey = "LIC-XYZ-123";
        builder.ApiClient.SalesResponses.Enqueue(BuildSalesPage([sale]));

        await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        var detail = Assert.Single(await builder.RevenueGumroadAdapter.GetAllAsync());
        Assert.Equal("LIC-XYZ-123", detail.LicenseKey);
        Assert.Equal("s1", detail.SaleId);
    }

    [Fact]
    public async Task SyncShop_NextPageKey_FollowsToSecondPage()
    {
        var builder = new GumroadSyncBuilder();
        var shop = await InsertShopAsync(builder);

        var firstPage = BuildSalesPage([BuildSale(id: "s1")], nextPageKey: "page-2-key");
        var secondPage = BuildSalesPage([BuildSale(id: "s2")]);
        builder.ApiClient.SalesResponses.Enqueue(firstPage);
        builder.ApiClient.SalesResponses.Enqueue(secondPage);

        var result = await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        Assert.Equal(2, result.NewRevenueRowCount);
        Assert.Equal(2, result.PagesFetched);
        Assert.Equal(2, builder.ApiClient.SalesCalls.Count);
        Assert.Null(builder.ApiClient.SalesCalls[0].PageKey);
        Assert.Equal("page-2-key", builder.ApiClient.SalesCalls[1].PageKey);
    }

    [Fact]
    public async Task SyncShop_NoNextPageKey_StopsPagination()
    {
        var builder = new GumroadSyncBuilder();
        var shop = await InsertShopAsync(builder);
        builder.ApiClient.SalesResponses.Enqueue(BuildSalesPage([BuildSale(id: "s1")]));

        await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        Assert.Single(builder.ApiClient.SalesCalls);
    }

    [Fact]
    public async Task SyncShop_DedupAcrossSyncs_OnlyInsertsNew()
    {
        var builder = new GumroadSyncBuilder();
        var shop = await InsertShopAsync(builder);
        builder.ApiClient.SalesResponses.Enqueue(BuildSalesPage([BuildSale(id: "s1")]));
        await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        builder.ApiClient.SalesResponses.Enqueue(BuildSalesPage([
            BuildSale(id: "s1"),
            BuildSale(id: "s2"),
        ]));
        var result = await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        Assert.Equal(1, result.NewRevenueRowCount);
        Assert.Equal(2, (await builder.RevenueAdapter.GetAllAsync()).Count());
    }

    [Fact]
    public async Task SyncShop_NoLastSync_AfterDateIsBackfillFloor()
    {
        var builder = new GumroadSyncBuilder();
        var shop = await InsertShopAsync(builder);
        shop.LastSyncDateTime = null;

        var before = DateTime.UtcNow.AddDays(-90).AddSeconds(-2);
        await builder.Service.SyncShopAsync(shop, CancellationToken.None);
        var after = DateTime.UtcNow.AddDays(-90).AddSeconds(2);

        Assert.Single(builder.ApiClient.SalesCalls);
        var afterDate = builder.ApiClient.SalesCalls[0].After;
        Assert.NotNull(afterDate);
        Assert.InRange(afterDate.Value, before, after);
    }

    [Fact]
    public async Task SyncShop_LastSyncSet_AfterDateUsesLastSync()
    {
        var builder = new GumroadSyncBuilder();
        var lastSync = new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc);
        var shop = await InsertShopAsync(builder);
        shop.LastSyncDateTime = lastSync;

        await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        Assert.Equal(lastSync, builder.ApiClient.SalesCalls[0].After);
    }

    [Fact]
    public async Task SyncShop_EmptySaleId_Skipped()
    {
        var builder = new GumroadSyncBuilder();
        var shop = await InsertShopAsync(builder);
        builder.ApiClient.SalesResponses.Enqueue(BuildSalesPage([
            BuildSale(id: ""),
            BuildSale(id: "valid"),
        ]));

        var result = await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        Assert.Equal(1, result.NewRevenueRowCount);
        var revenue = Assert.Single(await builder.RevenueAdapter.GetAllAsync());
        Assert.Equal("valid", revenue.PlatformTransactionId);
    }

    [Fact]
    public async Task SyncShop_MissingCurrency_DefaultsToUsd()
    {
        var builder = new GumroadSyncBuilder();
        var shop = await InsertShopAsync(builder);
        var sale = BuildSale(id: "s1");
        sale.Currency = string.Empty;
        builder.ApiClient.SalesResponses.Enqueue(BuildSalesPage([sale]));

        await builder.Service.SyncShopAsync(shop, CancellationToken.None);

        var revenue = Assert.Single(await builder.RevenueAdapter.GetAllAsync());
        Assert.Equal("usd", revenue.Currency);
    }

    [Fact]
    public async Task SyncShop_PlatformPropertyReturnsGumroad()
    {
        var builder = new GumroadSyncBuilder();

        Assert.Equal("Gumroad", builder.Service.Platform);
    }

    private static async Task<ShopConnection> InsertShopAsync(GumroadSyncBuilder builder)
    {
        return await builder.ShopConnectionAdapter.InsertAsync(new ShopConnection
        {
            AccessToken = "access-token",
            PlatformAccountId = "gum-user",
            PlatformConnectionId = 1,
            PlatformShopId = "gum-user",
            RefreshToken = null,
            ShopName = "Test Gumroad",
            Status = "Active",
            TokenExpiryDateTime = null,
            UserId = 42
        });
    }

    private static GumroadSalesResponse BuildSalesPage(IReadOnlyList<GumroadSale> sales, string? nextPageKey = null)
    {
        return new GumroadSalesResponse
        {
            NextPageKey = nextPageKey,
            Sales = sales.ToList(),
            Success = true
        };
    }

    private static GumroadSale BuildSale(string id, long priceCents = 1500, string currency = "usd")
    {
        return new GumroadSale
        {
            CreatedAt = new DateTime(2026, 4, 25, 12, 0, 0, DateTimeKind.Utc),
            Currency = currency,
            Disputed = false,
            Id = id,
            PriceCents = priceCents,
            ProductName = "Test Product",
            Refunded = false
        };
    }

    #endregion
}
