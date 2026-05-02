namespace My.Talli.Domain.Commands.Platforms;

using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class UpsertStripePayoutCommand
{
    #region <Constants>

    private const string StripePlatformName = "Stripe";

    #endregion

    #region <Variables>

    private readonly RepositoryAdapterAsync<Payout, ENTITIES.Payout> _payoutAdapter;
    private readonly RepositoryAdapterAsync<PayoutStripe, ENTITIES.PayoutStripe> _payoutStripeAdapter;

    #endregion

    #region <Constructors>

    public UpsertStripePayoutCommand(RepositoryAdapterAsync<Payout, ENTITIES.Payout> payoutAdapter, RepositoryAdapterAsync<PayoutStripe, ENTITIES.PayoutStripe> payoutStripeAdapter)
    {
        _payoutAdapter = payoutAdapter;
        _payoutStripeAdapter = payoutStripeAdapter;
    }

    #endregion

    #region <Methods>

    public async Task<int> ExecuteAsync(long userId, long shopConnectionId, IReadOnlyList<StripePayoutInput> inputs)
    {
        if (inputs.Count == 0)
            return 0;

        var payoutIds = inputs.Select(i => i.StripePayoutId).ToHashSet();
        var existing = (await _payoutAdapter.FindAsync(p =>
            p.UserId == userId &&
            p.Platform == StripePlatformName &&
            payoutIds.Contains(p.PlatformPayoutId))).ToList();

        var existingPayoutIds = existing
            .Select(p => p.PlatformPayoutId)
            .ToHashSet();

        var newInputs = inputs.Where(i => !existingPayoutIds.Contains(i.StripePayoutId)).ToList();
        if (newInputs.Count == 0)
            return 0;

        foreach (var input in newInputs)
        {
            var payout = await _payoutAdapter.InsertAsync(new Payout
            {
                Amount = input.Amount,
                Currency = input.Currency,
                ExpectedArrivalDate = input.ExpectedArrivalDate,
                PayoutDate = input.PayoutDate,
                Platform = StripePlatformName,
                PlatformPayoutId = input.StripePayoutId,
                ShopConnectionId = shopConnectionId,
                Status = input.Status,
                UserId = userId
            });

            await _payoutStripeAdapter.InsertAsync(new PayoutStripe
            {
                Id = payout.Id,
                PayoutMethod = input.PayoutMethod,
                StatementDescriptor = input.StatementDescriptor,
                StripePayoutId = input.StripePayoutId
            });
        }

        return newInputs.Count;
    }

    #endregion
}
