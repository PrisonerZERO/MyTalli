namespace My.Talli.Domain.Commands.Platforms;

using Domain.Components.Tokens;
using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class RefreshShopTokensCommand
{
    #region <Variables>

    private readonly RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> _shopConnectionAdapter;
    private readonly IShopTokenProtector _tokenProtector;

    #endregion

    #region <Constructors>

    public RefreshShopTokensCommand(RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> shopConnectionAdapter, IShopTokenProtector tokenProtector)
    {
        _shopConnectionAdapter = shopConnectionAdapter;
        _tokenProtector = tokenProtector;
    }

    #endregion

    #region <Methods>

    public async Task ExecuteAsync(long shopConnectionId, string accessToken, DateTime accessTokenExpiry, string? refreshToken, DateTime refreshTokenExpiry)
    {
        var shop = await _shopConnectionAdapter.GetByIdAsync(shopConnectionId)
            ?? throw new InvalidOperationException($"ShopConnection {shopConnectionId} not found.");

        shop.AccessToken = _tokenProtector.Protect(accessToken);
        shop.TokenExpiryDateTime = accessTokenExpiry;
        shop.RefreshToken = string.IsNullOrEmpty(refreshToken) ? shop.RefreshToken : _tokenProtector.Protect(refreshToken);
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
