namespace My.Talli.UnitTesting.Commands.Platforms;

using Domain.Commands.Platforms;
using My.Talli.UnitTesting.Infrastructure.Builders;

/// <summary>Tests</summary>
public class UpsertEtsyRevenueCommandTests
{
    #region <Methods>

    [Fact]
    public async Task Execute_EmptyInputs_ReturnsZeroAndInsertsNothing()
    {
        var builder = new PlatformHandlerBuilder();

        var inserted = await builder.UpsertRevenue.ExecuteAsync(userId: 1, shopConnectionId: 10, []);

        Assert.Equal(0, inserted);
        Assert.Empty(await builder.RevenueAdapter.GetAllAsync());
        Assert.Empty(await builder.RevenueEtsyAdapter.GetAllAsync());
    }

    [Fact]
    public async Task Execute_NewInputs_InsertsRevenueAndRevenueEtsyPair()
    {
        var builder = new PlatformHandlerBuilder();
        var input = BuildInput(transactionId: 555, listingId: 999, receiptId: 7777, gross: 25.00m);

        var inserted = await builder.UpsertRevenue.ExecuteAsync(1, 10, [input]);

        Assert.Equal(1, inserted);
        var revenue = Assert.Single(await builder.RevenueAdapter.GetAllAsync());
        Assert.Equal("Etsy", revenue.Platform);
        Assert.Equal("555", revenue.PlatformTransactionId);
        Assert.Equal(25.00m, revenue.GrossAmount);
        Assert.Equal(25.00m, revenue.NetAmount);
        Assert.Equal(0m, revenue.FeeAmount);
        Assert.Equal(10, revenue.ShopConnectionId);
        Assert.Equal(1, revenue.UserId);
        Assert.False(revenue.IsDisputed);
        Assert.False(revenue.IsRefunded);

        var detail = Assert.Single(await builder.RevenueEtsyAdapter.GetAllAsync());
        Assert.Equal(revenue.Id, detail.Id);
        Assert.Equal(999, detail.ListingId);
        Assert.Equal(7777, detail.ReceiptId);
    }

    [Fact]
    public async Task Execute_DuplicateTransactionId_SkipsExistingRow()
    {
        var builder = new PlatformHandlerBuilder();
        var first = BuildInput(transactionId: 555);
        await builder.UpsertRevenue.ExecuteAsync(1, 10, [first]);

        var inserted = await builder.UpsertRevenue.ExecuteAsync(1, 10, [BuildInput(transactionId: 555)]);

        Assert.Equal(0, inserted);
        Assert.Single(await builder.RevenueAdapter.GetAllAsync());
        Assert.Single(await builder.RevenueEtsyAdapter.GetAllAsync());
    }

    [Fact]
    public async Task Execute_MixedNewAndExistingInputs_InsertsOnlyNew()
    {
        var builder = new PlatformHandlerBuilder();
        await builder.UpsertRevenue.ExecuteAsync(1, 10, [BuildInput(transactionId: 100)]);

        var inserted = await builder.UpsertRevenue.ExecuteAsync(1, 10, [
            BuildInput(transactionId: 100),
            BuildInput(transactionId: 200),
            BuildInput(transactionId: 300)
        ]);

        Assert.Equal(2, inserted);
        Assert.Equal(3, (await builder.RevenueAdapter.GetAllAsync()).Count());
    }

    [Fact]
    public async Task Execute_SameTransactionIdAcrossUsers_TreatedIndependently()
    {
        var builder = new PlatformHandlerBuilder();
        await builder.UpsertRevenue.ExecuteAsync(userId: 1, 10, [BuildInput(transactionId: 555)]);

        var inserted = await builder.UpsertRevenue.ExecuteAsync(userId: 2, 20, [BuildInput(transactionId: 555)]);

        Assert.Equal(1, inserted);
        Assert.Equal(2, (await builder.RevenueAdapter.GetAllAsync()).Count());
    }

    private static EtsyRevenueInput BuildInput(long transactionId = 1, long listingId = 100, long receiptId = 1000, decimal gross = 10m)
    {
        return new EtsyRevenueInput
        {
            Currency = "USD",
            Description = "Test item",
            FeeAmount = 0m,
            GrossAmount = gross,
            ListingId = listingId,
            NetAmount = gross,
            ReceiptId = receiptId,
            ShopCurrency = "USD",
            TransactionDate = new DateTime(2026, 4, 25, 12, 0, 0, DateTimeKind.Utc),
            TransactionId = transactionId
        };
    }

    #endregion
}
