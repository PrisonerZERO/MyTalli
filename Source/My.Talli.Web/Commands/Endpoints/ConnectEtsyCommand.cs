namespace My.Talli.Web.Commands.Endpoints;

using Domain.Models;
using Domain.Repositories;
using Web.Services.Platforms;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class ConnectEtsyCommand
{
    #region <Constants>

    private const string EtsyPlatformName = "Etsy";

    #endregion

    #region <Variables>

    private readonly RepositoryAdapterAsync<PlatformConnection, ENTITIES.PlatformConnection> _platformConnectionAdapter;
    private readonly RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> _shopConnectionAdapter;
    private readonly RepositoryAdapterAsync<ShopConnectionEtsy, ENTITIES.ShopConnectionEtsy> _shopConnectionEtsyAdapter;

    #endregion

    #region <Constructors>

    public ConnectEtsyCommand(RepositoryAdapterAsync<PlatformConnection, ENTITIES.PlatformConnection> platformConnectionAdapter, RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> shopConnectionAdapter, RepositoryAdapterAsync<ShopConnectionEtsy, ENTITIES.ShopConnectionEtsy> shopConnectionEtsyAdapter)
    {
        _platformConnectionAdapter = platformConnectionAdapter;
        _shopConnectionAdapter = shopConnectionAdapter;
        _shopConnectionEtsyAdapter = shopConnectionEtsyAdapter;
    }

    #endregion

    #region <Methods>

    public async Task ExecuteAsync(long userId, EtsyTokenResponse tokens, string platformAccountId, IReadOnlyList<EtsyShop> shops)
    {
        var connection = await UpsertPlatformConnectionAsync(userId, tokens, platformAccountId);

        foreach (var shop in shops)
            await UpsertShopAsync(userId, connection.Id, shop);
    }

    private async Task<PlatformConnection> UpsertPlatformConnectionAsync(long userId, EtsyTokenResponse tokens, string platformAccountId)
    {
        var expiry = DateTime.UtcNow.AddSeconds(tokens.ExpiresIn);
        var existing = (await _platformConnectionAdapter.FindAsync(p => p.UserId == userId && p.Platform == EtsyPlatformName)).FirstOrDefault();

        if (existing is null)
        {
            return await _platformConnectionAdapter.InsertAsync(new PlatformConnection
            {
                AccessToken = tokens.AccessToken,
                ConnectionStatus = "Active",
                Platform = EtsyPlatformName,
                PlatformAccountId = platformAccountId,
                RefreshToken = tokens.RefreshToken,
                TokenExpiryDateTime = expiry,
                UserId = userId
            });
        }

        existing.AccessToken = tokens.AccessToken;
        existing.ConnectionStatus = "Active";
        existing.PlatformAccountId = platformAccountId;
        existing.RefreshToken = tokens.RefreshToken;
        existing.TokenExpiryDateTime = expiry;
        await _platformConnectionAdapter.UpdateAsync(existing);
        return existing;
    }

    private async Task UpsertShopAsync(long userId, long platformConnectionId, EtsyShop shop)
    {
        var platformShopId = shop.ShopId.ToString();
        var existing = (await _shopConnectionAdapter.FindAsync(s => s.PlatformConnectionId == platformConnectionId && s.PlatformShopId == platformShopId)).FirstOrDefault();

        if (existing is null)
        {
            existing = await _shopConnectionAdapter.InsertAsync(new ShopConnection
            {
                ConsecutiveFailures = 0,
                IsActive = true,
                IsEnabled = true,
                NextSyncDateTime = DateTime.UtcNow,
                PlatformConnectionId = platformConnectionId,
                PlatformShopId = platformShopId,
                ShopName = shop.ShopName ?? string.Empty,
                Status = "Pending",
                UserId = userId
            });
        }
        else
        {
            existing.IsActive = true;
            existing.ShopName = shop.ShopName ?? existing.ShopName;
            await _shopConnectionAdapter.UpdateAsync(existing);
        }

        await UpsertShopEtsyDetailAsync(existing.Id, shop);
    }

    private async Task UpsertShopEtsyDetailAsync(long shopConnectionId, EtsyShop shop)
    {
        var detail = await _shopConnectionEtsyAdapter.GetByIdAsync(shopConnectionId);

        if (detail is null)
        {
            await _shopConnectionEtsyAdapter.InsertAsync(new ShopConnectionEtsy
            {
                CountryCode = shop.CountryIso ?? string.Empty,
                Id = shopConnectionId,
                IsVacationMode = shop.IsVacation,
                ShopCurrency = shop.CurrencyCode ?? string.Empty,
                ShopUrl = shop.Url ?? string.Empty
            });
            return;
        }

        detail.CountryCode = shop.CountryIso ?? detail.CountryCode;
        detail.IsVacationMode = shop.IsVacation;
        detail.ShopCurrency = shop.CurrencyCode ?? detail.ShopCurrency;
        detail.ShopUrl = shop.Url ?? detail.ShopUrl;
        await _shopConnectionEtsyAdapter.UpdateAsync(detail);
    }

    #endregion
}
