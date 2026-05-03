namespace My.Talli.Domain.Commands.Platforms;

using Domain.Components;
using Domain.Components.Tokens;
using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class ConnectGumroadCommand
{
    #region <Constants>

    private const string GumroadPlatformName = "Gumroad";

    #endregion

    #region <Variables>

    private readonly RepositoryAdapterAsync<PlatformConnection, ENTITIES.PlatformConnection> _platformConnectionAdapter;
    private readonly RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> _shopConnectionAdapter;
    private readonly IShopTokenProtector _tokenProtector;

    #endregion

    #region <Constructors>

    public ConnectGumroadCommand(RepositoryAdapterAsync<PlatformConnection, ENTITIES.PlatformConnection> platformConnectionAdapter, RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> shopConnectionAdapter, IShopTokenProtector tokenProtector)
    {
        _platformConnectionAdapter = platformConnectionAdapter;
        _shopConnectionAdapter = shopConnectionAdapter;
        _tokenProtector = tokenProtector;
    }

    #endregion

    #region <Methods>

    public async Task<ConnectGumroadResult> ExecuteAsync(long userId, GumroadTokenResponse tokens, GumroadUser user)
    {
        var (connection, isFirstConnection) = await UpsertPlatformConnectionAsync(userId);
        var wasNewShop = await UpsertShopAsync(userId, connection.Id, tokens, user);

        return new ConnectGumroadResult
        {
            IsFirstConnection = isFirstConnection,
            WasNewShop = wasNewShop
        };
    }

    private async Task<(PlatformConnection Connection, bool WasInserted)> UpsertPlatformConnectionAsync(long userId)
    {
        var existing = (await _platformConnectionAdapter.FindAsync(p => p.UserId == userId && p.Platform == GumroadPlatformName)).FirstOrDefault();

        if (existing is null)
        {
            var inserted = await _platformConnectionAdapter.InsertAsync(new PlatformConnection
            {
                ConnectionStatus = "Active",
                Platform = GumroadPlatformName,
                UserId = userId
            });
            return (inserted, true);
        }

        existing.ConnectionStatus = "Active";
        await _platformConnectionAdapter.UpdateAsync(existing);
        return (existing, false);
    }

    private async Task<bool> UpsertShopAsync(long userId, long platformConnectionId, GumroadTokenResponse tokens, GumroadUser user)
    {
        var platformShopId = string.IsNullOrEmpty(user.UserId) ? user.Id : user.UserId;
        var shopName = string.IsNullOrEmpty(user.Name) ? (user.Email ?? "Gumroad") : user.Name;
        var now = DateTime.UtcNow;
        var existing = (await _shopConnectionAdapter.FindAsync(s => s.PlatformConnectionId == platformConnectionId && s.PlatformShopId == platformShopId)).FirstOrDefault();
        var wasNewShop = existing is null;

        var protectedAccessToken = _tokenProtector.Protect(tokens.AccessToken);

        if (existing is null)
        {
            await _shopConnectionAdapter.InsertAsync(new ShopConnection
            {
                AccessToken = protectedAccessToken,
                ConsecutiveFailures = 0,
                IsActive = true,
                IsEnabled = true,
                NextSyncDateTime = now,
                PlatformAccountId = platformShopId,
                PlatformConnectionId = platformConnectionId,
                PlatformShopId = platformShopId,
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
            existing.AccessToken = protectedAccessToken;
            existing.ConsecutiveFailures = 0;
            existing.IsActive = true;
            existing.LastErrorMessage = null;
            existing.NextSyncDateTime = now;
            existing.PlatformAccountId = platformShopId;
            existing.RefreshToken = null;
            existing.RefreshTokenExpiryDateTime = null;
            existing.ShopName = shopName;
            existing.Status = "Pending";
            existing.TokenExpiryDateTime = null;
            await _shopConnectionAdapter.UpdateAsync(existing);
        }

        return wasNewShop;
    }

    #endregion
}
