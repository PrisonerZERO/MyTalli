namespace My.Talli.UnitTesting.Commands.Platforms;

using Domain.Commands.Platforms;
using My.Talli.UnitTesting.Infrastructure.Builders;

/// <summary>Tests</summary>
public class UpsertStripeRevenueCommandTests
{
    #region <Methods>

    [Fact]
    public async Task Execute_EmptyInputs_ReturnsZeroAndInsertsNothing()
    {
        var builder = new PlatformHandlerBuilder();

        var inserted = await builder.UpsertStripeRevenue.ExecuteAsync(userId: 1, shopConnectionId: 10, []);

        Assert.Equal(0, inserted);
        Assert.Empty(await builder.RevenueAdapter.GetAllAsync());
        Assert.Empty(await builder.RevenueStripeAdapter.GetAllAsync());
    }

    [Fact]
    public async Task Execute_NewInputs_InsertsRevenueAndRevenueStripePair()
    {
        var builder = new PlatformHandlerBuilder();
        var input = BuildInput(balanceTxnId: "txn_1", gross: 100m, fee: 3m);

        var inserted = await builder.UpsertStripeRevenue.ExecuteAsync(1, 10, [input]);

        Assert.Equal(1, inserted);
        var revenue = Assert.Single(await builder.RevenueAdapter.GetAllAsync());
        Assert.Equal("Stripe", revenue.Platform);
        Assert.Equal("txn_1", revenue.PlatformTransactionId);
        Assert.Equal(100m, revenue.GrossAmount);
        Assert.Equal(3m, revenue.FeeAmount);
        Assert.Equal(97m, revenue.NetAmount);
        Assert.Equal(10, revenue.ShopConnectionId);
        Assert.Equal(1, revenue.UserId);

        var detail = Assert.Single(await builder.RevenueStripeAdapter.GetAllAsync());
        Assert.Equal(revenue.Id, detail.Id);
        Assert.Equal("txn_1", detail.BalanceTransactionId);
    }

    [Fact]
    public async Task Execute_RefundedAndDisputedFlags_StoredOnRevenue()
    {
        var builder = new PlatformHandlerBuilder();
        var input = BuildInput(balanceTxnId: "txn_1");
        input.IsRefunded = true;
        input.IsDisputed = true;

        await builder.UpsertStripeRevenue.ExecuteAsync(1, 10, [input]);

        var revenue = Assert.Single(await builder.RevenueAdapter.GetAllAsync());
        Assert.True(revenue.IsRefunded);
        Assert.True(revenue.IsDisputed);
    }

    [Fact]
    public async Task Execute_PaymentMethodAndRiskScore_StoredOnRevenueStripe()
    {
        var builder = new PlatformHandlerBuilder();
        var input = BuildInput(balanceTxnId: "txn_1");
        input.PaymentMethod = "card";
        input.RiskScore = 12;
        input.ExchangeRate = 1.05m;

        await builder.UpsertStripeRevenue.ExecuteAsync(1, 10, [input]);

        var detail = Assert.Single(await builder.RevenueStripeAdapter.GetAllAsync());
        Assert.Equal("card", detail.PaymentMethod);
        Assert.Equal(12, detail.RiskScore);
        Assert.Equal(1.05m, detail.ExchangeRate);
    }

    [Fact]
    public async Task Execute_DuplicateBalanceTransactionId_SkipsExistingRow()
    {
        var builder = new PlatformHandlerBuilder();
        await builder.UpsertStripeRevenue.ExecuteAsync(1, 10, [BuildInput(balanceTxnId: "txn_1")]);

        var inserted = await builder.UpsertStripeRevenue.ExecuteAsync(1, 10, [BuildInput(balanceTxnId: "txn_1")]);

        Assert.Equal(0, inserted);
        Assert.Single(await builder.RevenueAdapter.GetAllAsync());
        Assert.Single(await builder.RevenueStripeAdapter.GetAllAsync());
    }

    [Fact]
    public async Task Execute_MixedNewAndExistingInputs_InsertsOnlyNew()
    {
        var builder = new PlatformHandlerBuilder();
        await builder.UpsertStripeRevenue.ExecuteAsync(1, 10, [BuildInput(balanceTxnId: "txn_1")]);

        var inserted = await builder.UpsertStripeRevenue.ExecuteAsync(1, 10, [
            BuildInput(balanceTxnId: "txn_1"),
            BuildInput(balanceTxnId: "txn_2"),
            BuildInput(balanceTxnId: "txn_3")
        ]);

        Assert.Equal(2, inserted);
        Assert.Equal(3, (await builder.RevenueAdapter.GetAllAsync()).Count());
    }

    [Fact]
    public async Task Execute_SameBalanceTxnIdAcrossUsers_TreatedIndependently()
    {
        var builder = new PlatformHandlerBuilder();
        await builder.UpsertStripeRevenue.ExecuteAsync(userId: 1, 10, [BuildInput(balanceTxnId: "shared")]);

        var inserted = await builder.UpsertStripeRevenue.ExecuteAsync(userId: 2, 20, [BuildInput(balanceTxnId: "shared")]);

        Assert.Equal(1, inserted);
        Assert.Equal(2, (await builder.RevenueAdapter.GetAllAsync()).Count());
    }

    [Fact]
    public async Task Execute_NullableExchangeRate_StoresNull()
    {
        var builder = new PlatformHandlerBuilder();
        var input = BuildInput(balanceTxnId: "txn_1");
        input.ExchangeRate = null;

        await builder.UpsertStripeRevenue.ExecuteAsync(1, 10, [input]);

        var detail = Assert.Single(await builder.RevenueStripeAdapter.GetAllAsync());
        Assert.Null(detail.ExchangeRate);
    }

    private static StripeRevenueInput BuildInput(string balanceTxnId = "txn_1", decimal gross = 100m, decimal fee = 3m)
    {
        return new StripeRevenueInput
        {
            BalanceTransactionId = balanceTxnId,
            Currency = "usd",
            Description = "Test charge",
            FeeAmount = fee,
            GrossAmount = gross,
            NetAmount = gross - fee,
            PaymentMethod = "card",
            TransactionDate = new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc)
        };
    }

    #endregion
}
