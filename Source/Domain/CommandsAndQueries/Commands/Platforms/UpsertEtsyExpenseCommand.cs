namespace My.Talli.Domain.Commands.Platforms;

using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class UpsertEtsyExpenseCommand
{
    #region <Constants>

    private const string EtsyPlatformName = "Etsy";

    #endregion

    #region <Variables>

    private readonly RepositoryAdapterAsync<Expense, ENTITIES.Expense> _expenseAdapter;
    private readonly RepositoryAdapterAsync<ExpenseEtsy, ENTITIES.ExpenseEtsy> _expenseEtsyAdapter;

    #endregion

    #region <Constructors>

    public UpsertEtsyExpenseCommand(RepositoryAdapterAsync<Expense, ENTITIES.Expense> expenseAdapter, RepositoryAdapterAsync<ExpenseEtsy, ENTITIES.ExpenseEtsy> expenseEtsyAdapter)
    {
        _expenseAdapter = expenseAdapter;
        _expenseEtsyAdapter = expenseEtsyAdapter;
    }

    #endregion

    #region <Methods>

    public async Task<int> ExecuteAsync(long userId, long shopConnectionId, IReadOnlyList<EtsyExpenseInput> inputs)
    {
        if (inputs.Count == 0)
            return 0;

        var ledgerIds = inputs.Select(i => i.LedgerEntryId.ToString()).ToHashSet();
        var existing = (await _expenseAdapter.FindAsync(e =>
            e.UserId == userId &&
            e.Platform == EtsyPlatformName &&
            e.PlatformTransactionId != null &&
            ledgerIds.Contains(e.PlatformTransactionId))).ToList();

        var existingLedgerIds = existing
            .Select(e => e.PlatformTransactionId)
            .Where(id => id is not null)
            .ToHashSet();

        var newInputs = inputs.Where(i => !existingLedgerIds.Contains(i.LedgerEntryId.ToString())).ToList();
        if (newInputs.Count == 0)
            return 0;

        foreach (var input in newInputs)
        {
            var expense = await _expenseAdapter.InsertAsync(new Expense
            {
                Amount = input.Amount,
                Category = input.Category,
                Currency = input.Currency,
                Description = input.Description,
                ExpenseDate = input.ExpenseDate,
                Platform = EtsyPlatformName,
                PlatformTransactionId = input.LedgerEntryId.ToString(),
                ShopConnectionId = shopConnectionId,
                UserId = userId
            });

            await _expenseEtsyAdapter.InsertAsync(new ExpenseEtsy
            {
                AdCampaignId = input.AdCampaignId,
                Id = expense.Id,
                LedgerEntryId = input.LedgerEntryId,
                ListingId = input.ListingId
            });
        }

        return newInputs.Count;
    }

    #endregion
}
