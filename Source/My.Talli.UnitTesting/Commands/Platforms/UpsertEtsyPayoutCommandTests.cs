namespace My.Talli.UnitTesting.Commands.Platforms;

using Domain.Commands.Platforms;
using My.Talli.UnitTesting.Infrastructure.Builders;

/// <summary>Tests</summary>
public class UpsertEtsyPayoutCommandTests
{
    #region <Methods>

    [Fact]
    public async Task Execute_EmptyInputs_ReturnsZeroAndInsertsNothing()
    {
        var builder = new PlatformHandlerBuilder();

        var inserted = await builder.UpsertPayout.ExecuteAsync(userId: 1, shopConnectionId: 10, []);

        Assert.Equal(0, inserted);
        Assert.Empty(await builder.PayoutAdapter.GetAllAsync());
        Assert.Empty(await builder.PayoutEtsyAdapter.GetAllAsync());
    }

    [Fact]
    public async Task Execute_NewInputs_InsertsPayoutAndPayoutEtsyPair()
    {
        var builder = new PlatformHandlerBuilder();
        var input = BuildInput(ledgerEntryId: 8888, amount: 125.50m);

        var inserted = await builder.UpsertPayout.ExecuteAsync(1, 10, [input]);

        Assert.Equal(1, inserted);
        var payout = Assert.Single(await builder.PayoutAdapter.GetAllAsync());
        Assert.Equal("Etsy", payout.Platform);
        Assert.Equal("8888", payout.PlatformPayoutId);
        Assert.Equal(125.50m, payout.Amount);
        Assert.Equal("Paid", payout.Status);
        Assert.Equal(10, payout.ShopConnectionId);
        Assert.Equal(1, payout.UserId);

        var detail = Assert.Single(await builder.PayoutEtsyAdapter.GetAllAsync());
        Assert.Equal(payout.Id, detail.Id);
        Assert.Equal(8888, detail.LedgerEntryId);
        Assert.Equal("USD", detail.ShopCurrency);
    }

    [Fact]
    public async Task Execute_DuplicateLedgerEntryId_SkipsExistingRow()
    {
        var builder = new PlatformHandlerBuilder();
        await builder.UpsertPayout.ExecuteAsync(1, 10, [BuildInput(ledgerEntryId: 8888)]);

        var inserted = await builder.UpsertPayout.ExecuteAsync(1, 10, [BuildInput(ledgerEntryId: 8888)]);

        Assert.Equal(0, inserted);
        Assert.Single(await builder.PayoutAdapter.GetAllAsync());
    }

    [Fact]
    public async Task Execute_MixedNewAndExistingInputs_InsertsOnlyNew()
    {
        var builder = new PlatformHandlerBuilder();
        await builder.UpsertPayout.ExecuteAsync(1, 10, [BuildInput(ledgerEntryId: 1)]);

        var inserted = await builder.UpsertPayout.ExecuteAsync(1, 10, [
            BuildInput(ledgerEntryId: 1),
            BuildInput(ledgerEntryId: 2),
            BuildInput(ledgerEntryId: 3)
        ]);

        Assert.Equal(2, inserted);
        Assert.Equal(3, (await builder.PayoutAdapter.GetAllAsync()).Count());
    }

    [Fact]
    public async Task Execute_SameLedgerIdAcrossUsers_TreatedIndependently()
    {
        var builder = new PlatformHandlerBuilder();
        await builder.UpsertPayout.ExecuteAsync(userId: 1, 10, [BuildInput(ledgerEntryId: 8888)]);

        var inserted = await builder.UpsertPayout.ExecuteAsync(userId: 2, 20, [BuildInput(ledgerEntryId: 8888)]);

        Assert.Equal(1, inserted);
        Assert.Equal(2, (await builder.PayoutAdapter.GetAllAsync()).Count());
    }

    private static EtsyPayoutInput BuildInput(long ledgerEntryId = 1, decimal amount = 100m, string status = "Paid")
    {
        return new EtsyPayoutInput
        {
            Amount = amount,
            Currency = "USD",
            LedgerEntryId = ledgerEntryId,
            PayoutDate = new DateTime(2026, 4, 25, 12, 0, 0, DateTimeKind.Utc),
            ShopCurrency = "USD",
            Status = status
        };
    }

    #endregion
}
