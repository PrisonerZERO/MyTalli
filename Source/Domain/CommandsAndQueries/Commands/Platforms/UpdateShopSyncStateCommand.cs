namespace My.Talli.Domain.Commands.Platforms;

using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class UpdateShopSyncStateCommand
{
    #region <Constants>

    public const string StatusCompleted = "Completed";
    public const string StatusFailed = "Failed";
    public const string StatusInProgress = "InProgress";

    #endregion

    #region <Variables>

    private readonly RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> _shopConnectionAdapter;

    #endregion

    #region <Constructors>

    public UpdateShopSyncStateCommand(RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> shopConnectionAdapter)
    {
        _shopConnectionAdapter = shopConnectionAdapter;
    }

    #endregion

    #region <Methods>

    public async Task MarkInProgressAsync(long shopConnectionId)
    {
        var shop = await _shopConnectionAdapter.GetByIdAsync(shopConnectionId);
        if (shop is null)
            return;

        shop.Status = StatusInProgress;
        await _shopConnectionAdapter.UpdateAsync(shop);
    }

    public async Task MarkCompletedAsync(long shopConnectionId, DateTime syncedAt, DateTime nextSyncAt)
    {
        var shop = await _shopConnectionAdapter.GetByIdAsync(shopConnectionId);
        if (shop is null)
            return;

        shop.ConsecutiveFailures = 0;
        shop.LastErrorMessage = null;
        shop.LastSyncDateTime = syncedAt;
        shop.NextSyncDateTime = nextSyncAt;
        shop.Status = StatusCompleted;
        await _shopConnectionAdapter.UpdateAsync(shop);
    }

    public async Task MarkFailedAsync(long shopConnectionId, string errorMessage, DateTime nextSyncAt)
    {
        var shop = await _shopConnectionAdapter.GetByIdAsync(shopConnectionId);
        if (shop is null)
            return;

        shop.ConsecutiveFailures += 1;
        shop.LastErrorMessage = errorMessage;
        shop.NextSyncDateTime = nextSyncAt;
        shop.Status = StatusFailed;
        await _shopConnectionAdapter.UpdateAsync(shop);
    }

    #endregion
}
