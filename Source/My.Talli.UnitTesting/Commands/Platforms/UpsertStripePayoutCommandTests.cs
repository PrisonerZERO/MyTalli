namespace My.Talli.UnitTesting.Commands.Platforms;

using Domain.Commands.Platforms;
using My.Talli.UnitTesting.Infrastructure.Builders;

/// <summary>Tests</summary>
public class UpsertStripePayoutCommandTests
{
    #region <Methods>

    [Fact]
    public async Task Execute_EmptyInputs_ReturnsZeroAndInsertsNothing()
    {
        var builder = new PlatformHandlerBuilder();

        var inserted = await builder.UpsertStripePayout.ExecuteAsync(userId: 1, shopConnectionId: 10, []);

        Assert.Equal(0, inserted);
        Assert.Empty(await builder.PayoutAdapter.GetAllAsync());
        Assert.Empty(await builder.PayoutStripeAdapter.GetAllAsync());
    }

    [Fact]
    public async Task Execute_NewInputs_InsertsPayoutAndPayoutStripePair()
    {
        var builder = new PlatformHandlerBuilder();
        var input = BuildInput(payoutId: "po_1", amount: 250m);

        var inserted = await builder.UpsertStripePayout.ExecuteAsync(1, 10, [input]);

        Assert.Equal(1, inserted);
        var payout = Assert.Single(await builder.PayoutAdapter.GetAllAsync());
        Assert.Equal("Stripe", payout.Platform);
        Assert.Equal("po_1", payout.PlatformPayoutId);
        Assert.Equal(250m, payout.Amount);
        Assert.Equal(10, payout.ShopConnectionId);
        Assert.Equal(1, payout.UserId);

        var detail = Assert.Single(await builder.PayoutStripeAdapter.GetAllAsync());
        Assert.Equal(payout.Id, detail.Id);
        Assert.Equal("po_1", detail.StripePayoutId);
    }

    [Fact]
    public async Task Execute_PayoutMethodAndStatementDescriptor_StoredOnSubtable()
    {
        var builder = new PlatformHandlerBuilder();
        var input = BuildInput(payoutId: "po_1");
        input.PayoutMethod = "instant";
        input.StatementDescriptor = "STRIPE PAYOUT";

        await builder.UpsertStripePayout.ExecuteAsync(1, 10, [input]);

        var detail = Assert.Single(await builder.PayoutStripeAdapter.GetAllAsync());
        Assert.Equal("instant", detail.PayoutMethod);
        Assert.Equal("STRIPE PAYOUT", detail.StatementDescriptor);
    }

    [Fact]
    public async Task Execute_DuplicateStripePayoutId_SkipsExistingRow()
    {
        var builder = new PlatformHandlerBuilder();
        await builder.UpsertStripePayout.ExecuteAsync(1, 10, [BuildInput(payoutId: "po_1")]);

        var inserted = await builder.UpsertStripePayout.ExecuteAsync(1, 10, [BuildInput(payoutId: "po_1")]);

        Assert.Equal(0, inserted);
        Assert.Single(await builder.PayoutAdapter.GetAllAsync());
        Assert.Single(await builder.PayoutStripeAdapter.GetAllAsync());
    }

    [Fact]
    public async Task Execute_MixedNewAndExistingInputs_InsertsOnlyNew()
    {
        var builder = new PlatformHandlerBuilder();
        await builder.UpsertStripePayout.ExecuteAsync(1, 10, [BuildInput(payoutId: "po_1")]);

        var inserted = await builder.UpsertStripePayout.ExecuteAsync(1, 10, [
            BuildInput(payoutId: "po_1"),
            BuildInput(payoutId: "po_2"),
            BuildInput(payoutId: "po_3")
        ]);

        Assert.Equal(2, inserted);
        Assert.Equal(3, (await builder.PayoutAdapter.GetAllAsync()).Count());
    }

    [Fact]
    public async Task Execute_SamePayoutIdAcrossUsers_TreatedIndependently()
    {
        var builder = new PlatformHandlerBuilder();
        await builder.UpsertStripePayout.ExecuteAsync(userId: 1, 10, [BuildInput(payoutId: "shared")]);

        var inserted = await builder.UpsertStripePayout.ExecuteAsync(userId: 2, 20, [BuildInput(payoutId: "shared")]);

        Assert.Equal(1, inserted);
        Assert.Equal(2, (await builder.PayoutAdapter.GetAllAsync()).Count());
    }

    [Fact]
    public async Task Execute_NullableStatementDescriptor_StoresNull()
    {
        var builder = new PlatformHandlerBuilder();
        var input = BuildInput(payoutId: "po_1");
        input.StatementDescriptor = null;

        await builder.UpsertStripePayout.ExecuteAsync(1, 10, [input]);

        var detail = Assert.Single(await builder.PayoutStripeAdapter.GetAllAsync());
        Assert.Null(detail.StatementDescriptor);
    }

    [Fact]
    public async Task Execute_NullableExpectedArrivalDate_StoresNull()
    {
        var builder = new PlatformHandlerBuilder();
        var input = BuildInput(payoutId: "po_1");
        input.ExpectedArrivalDate = null;

        await builder.UpsertStripePayout.ExecuteAsync(1, 10, [input]);

        var payout = Assert.Single(await builder.PayoutAdapter.GetAllAsync());
        Assert.Null(payout.ExpectedArrivalDate);
    }

    private static StripePayoutInput BuildInput(string payoutId = "po_1", decimal amount = 250m)
    {
        return new StripePayoutInput
        {
            Amount = amount,
            Currency = "usd",
            ExpectedArrivalDate = new DateTime(2026, 5, 5, 12, 0, 0, DateTimeKind.Utc),
            PayoutDate = new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc),
            PayoutMethod = "standard",
            Status = "in_transit",
            StripePayoutId = payoutId
        };
    }

    #endregion
}
