namespace My.Talli.UnitTesting.Commands.Platforms;

using Domain.Commands.Platforms;
using My.Talli.UnitTesting.Infrastructure.Builders;

/// <summary>Tests</summary>
public class UpsertEtsyExpenseCommandTests
{
    #region <Methods>

    [Fact]
    public async Task Execute_EmptyInputs_ReturnsZeroAndInsertsNothing()
    {
        var builder = new PlatformHandlerBuilder();

        var inserted = await builder.UpsertExpense.ExecuteAsync(userId: 1, shopConnectionId: 10, []);

        Assert.Equal(0, inserted);
        Assert.Empty(await builder.ExpenseAdapter.GetAllAsync());
        Assert.Empty(await builder.ExpenseEtsyAdapter.GetAllAsync());
    }

    [Fact]
    public async Task Execute_NewInputs_InsertsExpenseAndExpenseEtsyPair()
    {
        var builder = new PlatformHandlerBuilder();
        var input = BuildInput(ledgerEntryId: 4242, amount: 0.20m, category: "Listing Fee", listingId: 999);

        var inserted = await builder.UpsertExpense.ExecuteAsync(1, 10, [input]);

        Assert.Equal(1, inserted);
        var expense = Assert.Single(await builder.ExpenseAdapter.GetAllAsync());
        Assert.Equal("Etsy", expense.Platform);
        Assert.Equal("4242", expense.PlatformTransactionId);
        Assert.Equal(0.20m, expense.Amount);
        Assert.Equal("Listing Fee", expense.Category);
        Assert.Equal(10, expense.ShopConnectionId);
        Assert.Equal(1, expense.UserId);

        var detail = Assert.Single(await builder.ExpenseEtsyAdapter.GetAllAsync());
        Assert.Equal(expense.Id, detail.Id);
        Assert.Equal(4242, detail.LedgerEntryId);
        Assert.Equal(999, detail.ListingId);
    }

    [Fact]
    public async Task Execute_DuplicateLedgerEntryId_SkipsExistingRow()
    {
        var builder = new PlatformHandlerBuilder();
        await builder.UpsertExpense.ExecuteAsync(1, 10, [BuildInput(ledgerEntryId: 4242)]);

        var inserted = await builder.UpsertExpense.ExecuteAsync(1, 10, [BuildInput(ledgerEntryId: 4242)]);

        Assert.Equal(0, inserted);
        Assert.Single(await builder.ExpenseAdapter.GetAllAsync());
    }

    [Fact]
    public async Task Execute_MixedNewAndExistingInputs_InsertsOnlyNew()
    {
        var builder = new PlatformHandlerBuilder();
        await builder.UpsertExpense.ExecuteAsync(1, 10, [BuildInput(ledgerEntryId: 1)]);

        var inserted = await builder.UpsertExpense.ExecuteAsync(1, 10, [
            BuildInput(ledgerEntryId: 1),
            BuildInput(ledgerEntryId: 2),
            BuildInput(ledgerEntryId: 3)
        ]);

        Assert.Equal(2, inserted);
        Assert.Equal(3, (await builder.ExpenseAdapter.GetAllAsync()).Count());
    }

    [Fact]
    public async Task Execute_NullableFieldsOmitted_PersistsAsNull()
    {
        var builder = new PlatformHandlerBuilder();
        var input = BuildInput(ledgerEntryId: 1, listingId: null, adCampaignId: null);

        await builder.UpsertExpense.ExecuteAsync(1, 10, [input]);

        var detail = Assert.Single(await builder.ExpenseEtsyAdapter.GetAllAsync());
        Assert.Null(detail.ListingId);
        Assert.Null(detail.AdCampaignId);
    }

    [Fact]
    public async Task Execute_SameLedgerIdAcrossUsers_TreatedIndependently()
    {
        var builder = new PlatformHandlerBuilder();
        await builder.UpsertExpense.ExecuteAsync(userId: 1, 10, [BuildInput(ledgerEntryId: 4242)]);

        var inserted = await builder.UpsertExpense.ExecuteAsync(userId: 2, 20, [BuildInput(ledgerEntryId: 4242)]);

        Assert.Equal(1, inserted);
        Assert.Equal(2, (await builder.ExpenseAdapter.GetAllAsync()).Count());
    }

    private static EtsyExpenseInput BuildInput(long ledgerEntryId = 1, decimal amount = 0.20m, string category = "Listing Fee", long? listingId = 100, long? adCampaignId = null)
    {
        return new EtsyExpenseInput
        {
            AdCampaignId = adCampaignId,
            Amount = amount,
            Category = category,
            Currency = "USD",
            Description = category,
            ExpenseDate = new DateTime(2026, 4, 25, 12, 0, 0, DateTimeKind.Utc),
            LedgerEntryId = ledgerEntryId,
            ListingId = listingId
        };
    }

    #endregion
}
