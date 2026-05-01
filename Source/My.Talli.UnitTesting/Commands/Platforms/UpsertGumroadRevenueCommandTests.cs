namespace My.Talli.UnitTesting.Commands.Platforms;

using Domain.Commands.Platforms;
using My.Talli.UnitTesting.Infrastructure.Builders;

/// <summary>Tests</summary>
public class UpsertGumroadRevenueCommandTests
{
    #region <Methods>

    [Fact]
    public async Task Execute_EmptyInputs_ReturnsZeroAndInsertsNothing()
    {
        var builder = new PlatformHandlerBuilder();

        var inserted = await builder.UpsertGumroadRevenue.ExecuteAsync(userId: 1, shopConnectionId: 10, []);

        Assert.Equal(0, inserted);
        Assert.Empty(await builder.RevenueAdapter.GetAllAsync());
        Assert.Empty(await builder.RevenueGumroadAdapter.GetAllAsync());
    }

    [Fact]
    public async Task Execute_NewInputs_InsertsRevenueAndRevenueGumroadPair()
    {
        var builder = new PlatformHandlerBuilder();
        var input = BuildInput(saleId: "sale-555", license: "LIC-AAA", gross: 25.00m);

        var inserted = await builder.UpsertGumroadRevenue.ExecuteAsync(1, 10, [input]);

        Assert.Equal(1, inserted);
        var revenue = Assert.Single(await builder.RevenueAdapter.GetAllAsync());
        Assert.Equal("Gumroad", revenue.Platform);
        Assert.Equal("sale-555", revenue.PlatformTransactionId);
        Assert.Equal(25.00m, revenue.GrossAmount);
        Assert.Equal(25.00m, revenue.NetAmount);
        Assert.Equal(0m, revenue.FeeAmount);
        Assert.Equal(10, revenue.ShopConnectionId);
        Assert.Equal(1, revenue.UserId);

        var detail = Assert.Single(await builder.RevenueGumroadAdapter.GetAllAsync());
        Assert.Equal(revenue.Id, detail.Id);
        Assert.Equal("sale-555", detail.SaleId);
        Assert.Equal("LIC-AAA", detail.LicenseKey);
    }

    [Fact]
    public async Task Execute_RefundedSale_StoresIsRefundedFlag()
    {
        var builder = new PlatformHandlerBuilder();
        var input = BuildInput(saleId: "s1");
        input.IsRefunded = true;

        await builder.UpsertGumroadRevenue.ExecuteAsync(1, 10, [input]);

        var revenue = Assert.Single(await builder.RevenueAdapter.GetAllAsync());
        Assert.True(revenue.IsRefunded);
    }

    [Fact]
    public async Task Execute_DisputedSale_StoresIsDisputedFlag()
    {
        var builder = new PlatformHandlerBuilder();
        var input = BuildInput(saleId: "s1");
        input.IsDisputed = true;

        await builder.UpsertGumroadRevenue.ExecuteAsync(1, 10, [input]);

        var revenue = Assert.Single(await builder.RevenueAdapter.GetAllAsync());
        Assert.True(revenue.IsDisputed);
    }

    [Fact]
    public async Task Execute_DuplicateSaleId_SkipsExistingRow()
    {
        var builder = new PlatformHandlerBuilder();
        await builder.UpsertGumroadRevenue.ExecuteAsync(1, 10, [BuildInput(saleId: "sale-1")]);

        var inserted = await builder.UpsertGumroadRevenue.ExecuteAsync(1, 10, [BuildInput(saleId: "sale-1")]);

        Assert.Equal(0, inserted);
        Assert.Single(await builder.RevenueAdapter.GetAllAsync());
        Assert.Single(await builder.RevenueGumroadAdapter.GetAllAsync());
    }

    [Fact]
    public async Task Execute_MixedNewAndExistingInputs_InsertsOnlyNew()
    {
        var builder = new PlatformHandlerBuilder();
        await builder.UpsertGumroadRevenue.ExecuteAsync(1, 10, [BuildInput(saleId: "sale-1")]);

        var inserted = await builder.UpsertGumroadRevenue.ExecuteAsync(1, 10, [
            BuildInput(saleId: "sale-1"),
            BuildInput(saleId: "sale-2"),
            BuildInput(saleId: "sale-3")
        ]);

        Assert.Equal(2, inserted);
        Assert.Equal(3, (await builder.RevenueAdapter.GetAllAsync()).Count());
    }

    [Fact]
    public async Task Execute_SameSaleIdAcrossUsers_TreatedIndependently()
    {
        var builder = new PlatformHandlerBuilder();
        await builder.UpsertGumroadRevenue.ExecuteAsync(userId: 1, 10, [BuildInput(saleId: "shared")]);

        var inserted = await builder.UpsertGumroadRevenue.ExecuteAsync(userId: 2, 20, [BuildInput(saleId: "shared")]);

        Assert.Equal(1, inserted);
        Assert.Equal(2, (await builder.RevenueAdapter.GetAllAsync()).Count());
    }

    [Fact]
    public async Task Execute_NullableLicenseKey_StoresNull()
    {
        var builder = new PlatformHandlerBuilder();
        var input = BuildInput(saleId: "s1", license: null);

        await builder.UpsertGumroadRevenue.ExecuteAsync(1, 10, [input]);

        var detail = Assert.Single(await builder.RevenueGumroadAdapter.GetAllAsync());
        Assert.Null(detail.LicenseKey);
    }

    private static GumroadRevenueInput BuildInput(string saleId = "sale-1", string? license = "LIC-1", decimal gross = 10m)
    {
        return new GumroadRevenueInput
        {
            Currency = "usd",
            Description = "Test product",
            FeeAmount = 0m,
            GrossAmount = gross,
            LicenseKey = license,
            NetAmount = gross,
            SaleId = saleId,
            TransactionDate = new DateTime(2026, 4, 25, 12, 0, 0, DateTimeKind.Utc)
        };
    }

    #endregion
}
