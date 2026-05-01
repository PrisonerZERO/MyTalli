namespace My.Talli.Domain.Commands.Platforms;

using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class UpsertGumroadRevenueCommand
{
    #region <Constants>

    private const string GumroadPlatformName = "Gumroad";

    #endregion

    #region <Variables>

    private readonly RepositoryAdapterAsync<Revenue, ENTITIES.Revenue> _revenueAdapter;
    private readonly RepositoryAdapterAsync<RevenueGumroad, ENTITIES.RevenueGumroad> _revenueGumroadAdapter;

    #endregion

    #region <Constructors>

    public UpsertGumroadRevenueCommand(RepositoryAdapterAsync<Revenue, ENTITIES.Revenue> revenueAdapter, RepositoryAdapterAsync<RevenueGumroad, ENTITIES.RevenueGumroad> revenueGumroadAdapter)
    {
        _revenueAdapter = revenueAdapter;
        _revenueGumroadAdapter = revenueGumroadAdapter;
    }

    #endregion

    #region <Methods>

    public async Task<int> ExecuteAsync(long userId, long shopConnectionId, IReadOnlyList<GumroadRevenueInput> inputs)
    {
        if (inputs.Count == 0)
            return 0;

        var saleIds = inputs.Select(i => i.SaleId).ToHashSet();
        var existing = (await _revenueAdapter.FindAsync(r =>
            r.UserId == userId &&
            r.Platform == GumroadPlatformName &&
            r.PlatformTransactionId != null &&
            saleIds.Contains(r.PlatformTransactionId))).ToList();

        var existingSaleIds = existing
            .Select(r => r.PlatformTransactionId)
            .Where(id => id is not null)
            .ToHashSet();

        var newInputs = inputs.Where(i => !existingSaleIds.Contains(i.SaleId)).ToList();
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
                Platform = GumroadPlatformName,
                PlatformTransactionId = input.SaleId,
                ShopConnectionId = shopConnectionId,
                TransactionDate = input.TransactionDate,
                UserId = userId
            });

            await _revenueGumroadAdapter.InsertAsync(new RevenueGumroad
            {
                DiscoverFee = input.DiscoverFee,
                Id = revenue.Id,
                LicenseKey = input.LicenseKey,
                SaleId = input.SaleId
            });
        }

        return newInputs.Count;
    }

    #endregion
}
