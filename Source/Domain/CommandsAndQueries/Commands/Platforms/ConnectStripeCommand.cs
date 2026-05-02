namespace My.Talli.Domain.Commands.Platforms;

using Domain.Components;
using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class ConnectStripeCommand
{
    #region <Constants>

    private const string StripePlatformName = "Stripe";

    #endregion

    #region <Variables>

    private readonly RepositoryAdapterAsync<PlatformConnection, ENTITIES.PlatformConnection> _platformConnectionAdapter;
    private readonly RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> _shopConnectionAdapter;

    #endregion

    #region <Constructors>

    public ConnectStripeCommand(RepositoryAdapterAsync<PlatformConnection, ENTITIES.PlatformConnection> platformConnectionAdapter, RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> shopConnectionAdapter)
    {
        _platformConnectionAdapter = platformConnectionAdapter;
        _shopConnectionAdapter = shopConnectionAdapter;
    }

    #endregion

    #region <Methods>

    public async Task<ConnectStripeResult> ExecuteAsync(long userId, StripeAccountInfo account)
    {
        var (connection, isFirstConnection) = await UpsertPlatformConnectionAsync(userId);
        var wasNewShop = await UpsertShopAsync(userId, connection.Id, account);

        return new ConnectStripeResult
        {
            IsFirstConnection = isFirstConnection,
            WasNewShop = wasNewShop
        };
    }

    private async Task<(PlatformConnection Connection, bool WasInserted)> UpsertPlatformConnectionAsync(long userId)
    {
        var existing = (await _platformConnectionAdapter.FindAsync(p => p.UserId == userId && p.Platform == StripePlatformName)).FirstOrDefault();

        if (existing is null)
        {
            var inserted = await _platformConnectionAdapter.InsertAsync(new PlatformConnection
            {
                ConnectionStatus = "Active",
                Platform = StripePlatformName,
                UserId = userId
            });
            return (inserted, true);
        }

        existing.ConnectionStatus = "Active";
        await _platformConnectionAdapter.UpdateAsync(existing);
        return (existing, false);
    }

    private async Task<bool> UpsertShopAsync(long userId, long platformConnectionId, StripeAccountInfo account)
    {
        var shopName = string.IsNullOrEmpty(account.BusinessName) ? (account.Email ?? "Stripe") : account.BusinessName;
        var now = DateTime.UtcNow;
        var existing = (await _shopConnectionAdapter.FindAsync(s => s.PlatformConnectionId == platformConnectionId && s.PlatformShopId == account.AccountId)).FirstOrDefault();
        var wasNewShop = existing is null;

        if (existing is null)
        {
            await _shopConnectionAdapter.InsertAsync(new ShopConnection
            {
                AccessToken = string.Empty,
                ConsecutiveFailures = 0,
                IsActive = true,
                IsEnabled = true,
                NextSyncDateTime = now,
                PlatformAccountId = account.AccountId,
                PlatformConnectionId = platformConnectionId,
                PlatformShopId = account.AccountId,
                RefreshToken = null,
                RefreshTokenExpiryDateTime = null,
                ShopName = shopName,
                Status = "Pending",
                TokenExpiryDateTime = null,
                UserId = userId
            });
        }
        else
        {
            existing.AccessToken = string.Empty;
            existing.IsActive = true;
            existing.PlatformAccountId = account.AccountId;
            existing.RefreshToken = null;
            existing.RefreshTokenExpiryDateTime = null;
            existing.ShopName = shopName;
            existing.TokenExpiryDateTime = null;
            await _shopConnectionAdapter.UpdateAsync(existing);
        }

        return wasNewShop;
    }

    #endregion
}
