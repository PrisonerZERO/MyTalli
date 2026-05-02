namespace My.Talli.Domain.Commands.Platforms;

using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class UpsertStripeRevenueCommand
{
    #region <Constants>

    private const string StripePlatformName = "Stripe";

    #endregion

    #region <Variables>

    private readonly RepositoryAdapterAsync<Revenue, ENTITIES.Revenue> _revenueAdapter;
    private readonly RepositoryAdapterAsync<RevenueStripe, ENTITIES.RevenueStripe> _revenueStripeAdapter;

    #endregion

    #region <Constructors>

    public UpsertStripeRevenueCommand(RepositoryAdapterAsync<Revenue, ENTITIES.Revenue> revenueAdapter, RepositoryAdapterAsync<RevenueStripe, ENTITIES.RevenueStripe> revenueStripeAdapter)
    {
        _revenueAdapter = revenueAdapter;
        _revenueStripeAdapter = revenueStripeAdapter;
    }

    #endregion

    #region <Methods>

    public async Task<int> ExecuteAsync(long userId, long shopConnectionId, IReadOnlyList<StripeRevenueInput> inputs)
    {
        if (inputs.Count == 0)
            return 0;

        var balanceTransactionIds = inputs.Select(i => i.BalanceTransactionId).ToHashSet();
        var existing = (await _revenueAdapter.FindAsync(r =>
            r.UserId == userId &&
            r.Platform == StripePlatformName &&
            r.PlatformTransactionId != null &&
            balanceTransactionIds.Contains(r.PlatformTransactionId))).ToList();

        var existingTxnIds = existing
            .Select(r => r.PlatformTransactionId)
            .Where(id => id is not null)
            .ToHashSet();

        var newInputs = inputs.Where(i => !existingTxnIds.Contains(i.BalanceTransactionId)).ToList();
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
                IsDisputed = input.IsDisputed,
                IsRefunded = input.IsRefunded,
                NetAmount = input.NetAmount,
                Platform = StripePlatformName,
                PlatformTransactionId = input.BalanceTransactionId,
                ShopConnectionId = shopConnectionId,
                TransactionDate = input.TransactionDate,
                UserId = userId
            });

            await _revenueStripeAdapter.InsertAsync(new RevenueStripe
            {
                BalanceTransactionId = input.BalanceTransactionId,
                ExchangeRate = input.ExchangeRate,
                Id = revenue.Id,
                PaymentMethod = input.PaymentMethod,
                RiskScore = input.RiskScore
            });
        }

        return newInputs.Count;
    }

    #endregion
}
