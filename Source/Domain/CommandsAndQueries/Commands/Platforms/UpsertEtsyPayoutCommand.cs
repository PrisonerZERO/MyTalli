namespace My.Talli.Domain.Commands.Platforms;

using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class UpsertEtsyPayoutCommand
{
    #region <Constants>

    private const string EtsyPlatformName = "Etsy";

    #endregion

    #region <Variables>

    private readonly RepositoryAdapterAsync<Payout, ENTITIES.Payout> _payoutAdapter;
    private readonly RepositoryAdapterAsync<PayoutEtsy, ENTITIES.PayoutEtsy> _payoutEtsyAdapter;

    #endregion

    #region <Constructors>

    public UpsertEtsyPayoutCommand(RepositoryAdapterAsync<Payout, ENTITIES.Payout> payoutAdapter, RepositoryAdapterAsync<PayoutEtsy, ENTITIES.PayoutEtsy> payoutEtsyAdapter)
    {
        _payoutAdapter = payoutAdapter;
        _payoutEtsyAdapter = payoutEtsyAdapter;
    }

    #endregion

    #region <Methods>

    public async Task<int> ExecuteAsync(long userId, long shopConnectionId, IReadOnlyList<EtsyPayoutInput> inputs)
    {
        if (inputs.Count == 0)
            return 0;

        var ledgerIds = inputs.Select(i => i.LedgerEntryId.ToString()).ToHashSet();
        var existing = (await _payoutAdapter.FindAsync(p =>
            p.UserId == userId &&
            p.Platform == EtsyPlatformName &&
            ledgerIds.Contains(p.PlatformPayoutId))).ToList();

        var existingLedgerIds = existing
            .Select(p => p.PlatformPayoutId)
            .ToHashSet();

        var newInputs = inputs.Where(i => !existingLedgerIds.Contains(i.LedgerEntryId.ToString())).ToList();
        if (newInputs.Count == 0)
            return 0;

        foreach (var input in newInputs)
        {
            var payout = await _payoutAdapter.InsertAsync(new Payout
            {
                Amount = input.Amount,
                Currency = input.Currency,
                PayoutDate = input.PayoutDate,
                Platform = EtsyPlatformName,
                PlatformPayoutId = input.LedgerEntryId.ToString(),
                ShopConnectionId = shopConnectionId,
                Status = input.Status,
                UserId = userId
            });

            await _payoutEtsyAdapter.InsertAsync(new PayoutEtsy
            {
                Id = payout.Id,
                LedgerEntryId = input.LedgerEntryId,
                ShopCurrency = input.ShopCurrency
            });
        }

        return newInputs.Count;
    }

    #endregion
}
