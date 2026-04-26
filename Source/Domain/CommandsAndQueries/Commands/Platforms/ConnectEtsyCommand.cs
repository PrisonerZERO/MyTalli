namespace My.Talli.Domain.Commands.Platforms;

using Domain.Components;
using Domain.Components.Tokens;
using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class ConnectEtsyCommand
{
    #region <Constants>

    private const string EtsyPlatformName = "Etsy";
    private const int EtsyRefreshTokenLifetimeDays = 90;

    #endregion

    #region <Variables>

    private readonly RepositoryAdapterAsync<PlatformConnection, ENTITIES.PlatformConnection> _platformConnectionAdapter;
    private readonly RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> _shopConnectionAdapter;
    private readonly RepositoryAdapterAsync<ShopConnectionEtsy, ENTITIES.ShopConnectionEtsy> _shopConnectionEtsyAdapter;
    private readonly IShopTokenProtector _tokenProtector;

    #endregion

    #region <Constructors>

    public ConnectEtsyCommand(RepositoryAdapterAsync<PlatformConnection, ENTITIES.PlatformConnection> platformConnectionAdapter, RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> shopConnectionAdapter, RepositoryAdapterAsync<ShopConnectionEtsy, ENTITIES.ShopConnectionEtsy> shopConnectionEtsyAdapter, IShopTokenProtector tokenProtector)
    {
        _platformConnectionAdapter = platformConnectionAdapter;
        _shopConnectionAdapter = shopConnectionAdapter;
        _shopConnectionEtsyAdapter = shopConnectionEtsyAdapter;
        _tokenProtector = tokenProtector;
    }

    #endregion

    #region <Methods>

    public async Task<ConnectEtsyResult> ExecuteAsync(long userId, EtsyTokenResponse tokens, string platformAccountId, IReadOnlyList<EtsyShop> shops)
    {
        var (connection, isFirstConnection) = await UpsertPlatformConnectionAsync(userId);

        var result = new ConnectEtsyResult { IsFirstConnection = isFirstConnection };

        foreach (var shop in shops)
        {
            var wasNewShop = await UpsertShopAsync(userId, connection.Id, tokens, platformAccountId, shop);
            if (wasNewShop)
                result.NewShopCount++;
            else
                result.RefreshedShopCount++;
        }

        return result;
    }

    private async Task<(PlatformConnection Connection, bool WasInserted)> UpsertPlatformConnectionAsync(long userId)
    {
        var existing = (await _platformConnectionAdapter.FindAsync(p => p.UserId == userId && p.Platform == EtsyPlatformName)).FirstOrDefault();

        if (existing is null)
        {
            var inserted = await _platformConnectionAdapter.InsertAsync(new PlatformConnection
            {
                ConnectionStatus = "Active",
                Platform = EtsyPlatformName,
                UserId = userId
            });
            return (inserted, true);
        }

        existing.ConnectionStatus = "Active";
        await _platformConnectionAdapter.UpdateAsync(existing);
        return (existing, false);
    }

    private async Task<bool> UpsertShopAsync(long userId, long platformConnectionId, EtsyTokenResponse tokens, string platformAccountId, EtsyShop shop)
    {
        var platformShopId = shop.ShopId.ToString();
        var now = DateTime.UtcNow;
        var accessExpiry = now.AddSeconds(tokens.ExpiresIn);
        var refreshExpiry = now.AddDays(EtsyRefreshTokenLifetimeDays);
        var existing = (await _shopConnectionAdapter.FindAsync(s => s.PlatformConnectionId == platformConnectionId && s.PlatformShopId == platformShopId)).FirstOrDefault();
        var wasNewShop = existing is null;

        var protectedAccessToken = _tokenProtector.Protect(tokens.AccessToken);
        var protectedRefreshToken = string.IsNullOrEmpty(tokens.RefreshToken) ? tokens.RefreshToken : _tokenProtector.Protect(tokens.RefreshToken);

        if (existing is null)
        {
            existing = await _shopConnectionAdapter.InsertAsync(new ShopConnection
            {
                AccessToken = protectedAccessToken,
                ConsecutiveFailures = 0,
                IsActive = true,
                IsEnabled = true,
                NextSyncDateTime = now,
                PlatformAccountId = platformAccountId,
                PlatformConnectionId = platformConnectionId,
                PlatformShopId = platformShopId,
                RefreshToken = protectedRefreshToken,
                RefreshTokenExpiryDateTime = refreshExpiry,
                ShopName = shop.ShopName ?? string.Empty,
                Status = "Pending",
                TokenExpiryDateTime = accessExpiry,
                UserId = userId
            });
        }
        else
        {
            existing.AccessToken = protectedAccessToken;
            existing.IsActive = true;
            existing.PlatformAccountId = platformAccountId;
            existing.RefreshToken = protectedRefreshToken;
            existing.RefreshTokenExpiryDateTime = refreshExpiry;
            existing.ShopName = shop.ShopName ?? existing.ShopName;
            existing.TokenExpiryDateTime = accessExpiry;
            await _shopConnectionAdapter.UpdateAsync(existing);
        }

        await UpsertShopEtsyDetailAsync(existing.Id, shop);
        return wasNewShop;
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
