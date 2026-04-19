namespace My.Talli.Domain.Commands.Platforms;

using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class UpsertEtsyRevenueCommand
{
    #region <Constants>

    private const string EtsyPlatformName = "Etsy";

    #endregion

    #region <Variables>

    private readonly RepositoryAdapterAsync<Revenue, ENTITIES.Revenue> _revenueAdapter;
    private readonly RepositoryAdapterAsync<RevenueEtsy, ENTITIES.RevenueEtsy> _revenueEtsyAdapter;

    #endregion

    #region <Constructors>

    public UpsertEtsyRevenueCommand(RepositoryAdapterAsync<Revenue, ENTITIES.Revenue> revenueAdapter, RepositoryAdapterAsync<RevenueEtsy, ENTITIES.RevenueEtsy> revenueEtsyAdapter)
    {
        _revenueAdapter = revenueAdapter;
        _revenueEtsyAdapter = revenueEtsyAdapter;
    }

    #endregion

    #region <Methods>

    public async Task<int> ExecuteAsync(long userId, long shopConnectionId, IReadOnlyList<EtsyRevenueInput> inputs)
    {
        if (inputs.Count == 0)
            return 0;

        var transactionIds = inputs.Select(i => i.TransactionId.ToString()).ToHashSet();
        var existing = (await _revenueAdapter.FindAsync(r =>
            r.UserId == userId &&
            r.Platform == EtsyPlatformName &&
            r.PlatformTransactionId != null &&
            transactionIds.Contains(r.PlatformTransactionId))).ToList();

        var existingTxnIds = existing
            .Select(r => r.PlatformTransactionId)
            .Where(id => id is not null)
            .ToHashSet();

        var newInputs = inputs.Where(i => !existingTxnIds.Contains(i.TransactionId.ToString())).ToList();
        if (newInputs.Count == 0)
            return 0;

        foreach (var input in newInputs)
        {
            var revenue = await _revenueAdapter.InsertAsync(new Revenue
            {
                Currency = input.Currency,
                Description = input.Description,
                FeeAmount = input.FeeAmount,
                GrossAmount = input.GrossAmount,
                IsDisputed = false,
                IsRefunded = false,
                NetAmount = input.NetAmount,
                Platform = EtsyPlatformName,
                PlatformTransactionId = input.TransactionId.ToString(),
                ShopConnectionId = shopConnectionId,
                TransactionDate = input.TransactionDate,
                UserId = userId
            });

            await _revenueEtsyAdapter.InsertAsync(new RevenueEtsy
            {
                Id = revenue.Id,
                ListingId = input.ListingId,
                ReceiptId = input.ReceiptId,
                ShopCurrency = input.ShopCurrency
            });
        }

        return newInputs.Count;
    }

    #endregion
}
