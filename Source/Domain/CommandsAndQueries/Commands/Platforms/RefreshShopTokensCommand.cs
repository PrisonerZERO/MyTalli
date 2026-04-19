namespace My.Talli.Domain.Commands.Platforms;

using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class RefreshShopTokensCommand
{
    #region <Variables>

    private readonly RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> _shopConnectionAdapter;

    #endregion

    #region <Constructors>

    public RefreshShopTokensCommand(RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> shopConnectionAdapter)
    {
        _shopConnectionAdapter = shopConnectionAdapter;
    }

    #endregion

    #region <Methods>

    public async Task ExecuteAsync(long shopConnectionId, string accessToken, DateTime accessTokenExpiry, string? refreshToken, DateTime refreshTokenExpiry)
    {
        var shop = await _shopConnectionAdapter.GetByIdAsync(shopConnectionId)
            ?? throw new InvalidOperationException($"ShopConnection {shopConnectionId} not found.");

        shop.AccessToken = accessToken;
        shop.TokenExpiryDateTime = accessTokenExpiry;
        shop.RefreshToken = refreshToken ?? shop.RefreshToken;
        shop.RefreshTokenExpiryDateTime = refreshTokenExpiry;
        shop.LastErrorMessage = null;

        await _shopConnectionAdapter.UpdateAsync(shop);
    }

    public async Task RecordFailureAsync(long shopConnectionId, string errorMessage)
    {
        var shop = await _shopConnectionAdapter.GetByIdAsync(shopConnectionId);
        if (shop is null)
            return;

        shop.LastErrorMessage = errorMessage;
        await _shopConnectionAdapter.UpdateAsync(shop);
    }

    #endregion
}
