namespace My.Talli.UnitTesting.Commands.Platforms;

using Domain.Components;
using My.Talli.UnitTesting.Infrastructure.Builders;

/// <summary>Tests</summary>
public class ConnectEtsyCommandTests
{
    #region <Methods>

    [Fact]
    public async Task Execute_NewConnection_InsertsPlatformConnection()
    {
        var builder = new PlatformHandlerBuilder();
        var tokens = BuildTokens();
        var shops = new List<EtsyShop> { BuildShop() };

        await builder.Command.ExecuteAsync(userId: 42, tokens, platformAccountId: "1226690893", shops);

        var connections = await builder.PlatformConnectionAdapter.GetAllAsync();
        var connection = Assert.Single(connections);
        Assert.Equal(42, connection.UserId);
        Assert.Equal("Etsy", connection.Platform);
        Assert.Equal("Active", connection.ConnectionStatus);
    }

    [Fact]
    public async Task Execute_NewConnection_StoresTokensOnShopConnection()
    {
        var builder = new PlatformHandlerBuilder();
        var tokens = BuildTokens();

        await builder.Command.ExecuteAsync(42, tokens, "1226690893", [BuildShop()]);

        var shop = Assert.Single(await builder.ShopConnectionAdapter.GetAllAsync());
        Assert.Equal("access-abc", builder.TokenProtector.Unprotect(shop.AccessToken));
        Assert.Equal("refresh-xyz", builder.TokenProtector.Unprotect(shop.RefreshToken!));
        Assert.Equal("1226690893", shop.PlatformAccountId);
    }

    [Fact]
    public async Task Execute_NewConnection_StoredTokensAreEncrypted()
    {
        var builder = new PlatformHandlerBuilder();

        await builder.Command.ExecuteAsync(42, BuildTokens("plain-access", "plain-refresh"), "1226690893", [BuildShop()]);

        var shop = Assert.Single(await builder.ShopConnectionAdapter.GetAllAsync());
        Assert.NotEqual("plain-access", shop.AccessToken);
        Assert.NotEqual("plain-refresh", shop.RefreshToken);
    }

    [Fact]
    public async Task Execute_NewConnection_SetsTokenExpiryRelativeToNow()
    {
        var builder = new PlatformHandlerBuilder();
        var tokens = BuildTokens(expiresIn: 3600);

        var before = DateTime.UtcNow;
        await builder.Command.ExecuteAsync(1, tokens, "user", [BuildShop()]);
        var after = DateTime.UtcNow;

        var shop = (await builder.ShopConnectionAdapter.GetAllAsync()).Single();
        Assert.NotNull(shop.TokenExpiryDateTime);
        Assert.InRange(shop.TokenExpiryDateTime!.Value, before.AddSeconds(3600 - 2), after.AddSeconds(3600 + 2));
    }

    [Fact]
    public async Task Execute_ExistingShop_UpdatesTokensInPlace()
    {
        var builder = new PlatformHandlerBuilder();
        await builder.Command.ExecuteAsync(1, BuildTokens("old-access", "old-refresh"), "user", [BuildShop()]);

        await builder.Command.ExecuteAsync(1, BuildTokens("new-access", "new-refresh"), "user", [BuildShop()]);

        var shop = Assert.Single(await builder.ShopConnectionAdapter.GetAllAsync());
        Assert.Equal("new-access", builder.TokenProtector.Unprotect(shop.AccessToken));
        Assert.Equal("new-refresh", builder.TokenProtector.Unprotect(shop.RefreshToken!));
    }

    [Fact]
    public async Task Execute_SecondEtsyLoginNewShop_KeepsFirstShopTokensIntact()
    {
        var builder = new PlatformHandlerBuilder();
        await builder.Command.ExecuteAsync(1, BuildTokens("login-a-access", "login-a-refresh"), "etsy-user-a", [BuildShop(shopId: 100, shopName: "Shop A")]);

        await builder.Command.ExecuteAsync(1, BuildTokens("login-b-access", "login-b-refresh"), "etsy-user-b", [BuildShop(shopId: 200, shopName: "Shop B")]);

        Assert.Single(await builder.PlatformConnectionAdapter.GetAllAsync());

        var shops = (await builder.ShopConnectionAdapter.GetAllAsync()).OrderBy(s => s.PlatformShopId).ToList();
        Assert.Equal(2, shops.Count);
        Assert.Equal("login-a-access", builder.TokenProtector.Unprotect(shops[0].AccessToken));
        Assert.Equal("etsy-user-a", shops[0].PlatformAccountId);
        Assert.Equal("login-b-access", builder.TokenProtector.Unprotect(shops[1].AccessToken));
        Assert.Equal("etsy-user-b", shops[1].PlatformAccountId);
    }

    [Fact]
    public async Task Execute_NewConnection_InsertsShopConnectionAndEtsyDetail()
    {
        var builder = new PlatformHandlerBuilder();
        var shop = BuildShop(shopId: 65266817, shopName: "MyTalliTestShop", currency: "USD", country: "US", url: "https://www.etsy.com/shop/MyTalliTestShop", vacation: false);

        await builder.Command.ExecuteAsync(42, BuildTokens(), "1226690893", [shop]);

        var shopConnection = Assert.Single(await builder.ShopConnectionAdapter.GetAllAsync());
        Assert.Equal("65266817", shopConnection.PlatformShopId);
        Assert.Equal("MyTalliTestShop", shopConnection.ShopName);
        Assert.Equal("Pending", shopConnection.Status);
        Assert.True(shopConnection.IsActive);
        Assert.True(shopConnection.IsEnabled);
        Assert.Equal(42, shopConnection.UserId);

        var shopEtsy = Assert.Single(await builder.ShopConnectionEtsyAdapter.GetAllAsync());
        Assert.Equal("US", shopEtsy.CountryCode);
        Assert.Equal("USD", shopEtsy.ShopCurrency);
        Assert.Equal("https://www.etsy.com/shop/MyTalliTestShop", shopEtsy.ShopUrl);
        Assert.False(shopEtsy.IsVacationMode);
    }

    [Fact]
    public async Task Execute_NewConnection_ShopEtsyIdEqualsShopConnectionId()
    {
        var builder = new PlatformHandlerBuilder();

        await builder.Command.ExecuteAsync(1, BuildTokens(), "user", [BuildShop()]);

        var shopConnection = (await builder.ShopConnectionAdapter.GetAllAsync()).Single();
        var shopEtsy = (await builder.ShopConnectionEtsyAdapter.GetAllAsync()).Single();
        Assert.Equal(shopConnection.Id, shopEtsy.Id);
    }

    [Fact]
    public async Task Execute_ShopInitialNextSyncIsSetToNow()
    {
        var builder = new PlatformHandlerBuilder();

        var before = DateTime.UtcNow;
        await builder.Command.ExecuteAsync(1, BuildTokens(), "user", [BuildShop()]);
        var after = DateTime.UtcNow;

        var shopConnection = (await builder.ShopConnectionAdapter.GetAllAsync()).Single();
        Assert.InRange(shopConnection.NextSyncDateTime, before.AddSeconds(-2), after.AddSeconds(2));
    }

    [Fact]
    public async Task Execute_ExistingShop_UpdatesNameAndEtsyDetailsWithoutCreatingNewRow()
    {
        var builder = new PlatformHandlerBuilder();
        await builder.Command.ExecuteAsync(1, BuildTokens(), "user", [BuildShop(shopName: "OldName", vacation: false)]);

        await builder.Command.ExecuteAsync(1, BuildTokens(), "user", [BuildShop(shopName: "NewName", vacation: true)]);

        var shopConnections = await builder.ShopConnectionAdapter.GetAllAsync();
        Assert.Single(shopConnections);
        Assert.Equal("NewName", shopConnections.Single().ShopName);

        var shopEtsyRows = await builder.ShopConnectionEtsyAdapter.GetAllAsync();
        Assert.Single(shopEtsyRows);
        Assert.True(shopEtsyRows.Single().IsVacationMode);
    }

    [Fact]
    public async Task Execute_FirstConnectionWithNewShop_ReportsFirstConnectionAndOneNewShop()
    {
        var builder = new PlatformHandlerBuilder();

        var result = await builder.Command.ExecuteAsync(1, BuildTokens(), "user", [BuildShop()]);

        Assert.True(result.IsFirstConnection);
        Assert.Equal(1, result.NewShopCount);
        Assert.Equal(0, result.RefreshedShopCount);
    }

    [Fact]
    public async Task Execute_ReconnectExistingShop_ReportsRefreshOnly()
    {
        var builder = new PlatformHandlerBuilder();
        await builder.Command.ExecuteAsync(1, BuildTokens(), "user", [BuildShop()]);

        var result = await builder.Command.ExecuteAsync(1, BuildTokens(), "user", [BuildShop()]);

        Assert.False(result.IsFirstConnection);
        Assert.Equal(0, result.NewShopCount);
        Assert.Equal(1, result.RefreshedShopCount);
    }

    [Fact]
    public async Task Execute_SecondEtsyLogin_ReportsOneNewShopAndZeroRefresh()
    {
        var builder = new PlatformHandlerBuilder();
        await builder.Command.ExecuteAsync(1, BuildTokens(), "etsy-user-a", [BuildShop(shopId: 100, shopName: "Shop A")]);

        var result = await builder.Command.ExecuteAsync(1, BuildTokens(), "etsy-user-b", [BuildShop(shopId: 200, shopName: "Shop B")]);

        Assert.False(result.IsFirstConnection);
        Assert.Equal(1, result.NewShopCount);
        Assert.Equal(0, result.RefreshedShopCount);
    }

    [Fact]
    public async Task Execute_NoShopsReturned_CreatesPlatformConnectionOnly()
    {
        var builder = new PlatformHandlerBuilder();

        await builder.Command.ExecuteAsync(1, BuildTokens(), "user", []);

        Assert.Single(await builder.PlatformConnectionAdapter.GetAllAsync());
        Assert.Empty(await builder.ShopConnectionAdapter.GetAllAsync());
        Assert.Empty(await builder.ShopConnectionEtsyAdapter.GetAllAsync());
    }

    [Fact]
    public async Task Execute_MultipleShops_CreatesOneRowPerShop()
    {
        var builder = new PlatformHandlerBuilder();
        var shops = new List<EtsyShop>
        {
            BuildShop(shopId: 100, shopName: "Shop A"),
            BuildShop(shopId: 200, shopName: "Shop B"),
        };

        await builder.Command.ExecuteAsync(1, BuildTokens(), "user", shops);

        var shopConnections = (await builder.ShopConnectionAdapter.GetAllAsync()).ToList();
        Assert.Equal(2, shopConnections.Count);
        Assert.Contains(shopConnections, s => s.PlatformShopId == "100");
        Assert.Contains(shopConnections, s => s.PlatformShopId == "200");
        Assert.Equal(2, (await builder.ShopConnectionEtsyAdapter.GetAllAsync()).Count());
    }

    [Fact]
    public async Task Execute_ExistingShopWithFailures_ResetsConsecutiveFailuresAndClearsErrorMessage()
    {
        var builder = new PlatformHandlerBuilder();

        // First connect — establishes the shop row
        await builder.Command.ExecuteAsync(1, BuildTokens(), "user", [BuildShop(shopId: 100)]);

        // Simulate the worker recording failures over time
        var stored = (await builder.ShopConnectionAdapter.GetAllAsync()).Single();
        stored.ConsecutiveFailures = 6;
        stored.LastErrorMessage = "An error occurred during a cryptographic operation";
        stored.Status = "Failed";
        await builder.ShopConnectionAdapter.UpdateAsync(stored);

        // Reconnect — should clear the failure state on the existing row
        await builder.Command.ExecuteAsync(1, BuildTokens(), "user", [BuildShop(shopId: 100)]);

        var refreshed = (await builder.ShopConnectionAdapter.GetAllAsync()).Single();
        Assert.Equal(0, refreshed.ConsecutiveFailures);
        Assert.Null(refreshed.LastErrorMessage);
        Assert.Equal("Pending", refreshed.Status);
    }

    [Fact]
    public async Task Execute_ExistingShopReconnect_RewritesEncryptedTokens()
    {
        var builder = new PlatformHandlerBuilder();
        await builder.Command.ExecuteAsync(1, BuildTokens(accessToken: "old-access", refreshToken: "old-refresh"), "user", [BuildShop(shopId: 100)]);

        await builder.Command.ExecuteAsync(1, BuildTokens(accessToken: "new-access", refreshToken: "new-refresh"), "user", [BuildShop(shopId: 100)]);

        var shop = (await builder.ShopConnectionAdapter.GetAllAsync()).Single();
        Assert.Equal("new-access", builder.TokenProtector.Unprotect(shop.AccessToken));
        Assert.Equal("new-refresh", builder.TokenProtector.Unprotect(shop.RefreshToken!));
    }

    [Fact]
    public async Task Execute_ShopWithNullFields_DefaultsToEmptyStrings()
    {
        var builder = new PlatformHandlerBuilder();
        var shop = new EtsyShop { ShopId = 999, ShopName = null, CountryIso = null, CurrencyCode = null, Url = null };

        await builder.Command.ExecuteAsync(1, BuildTokens(), "user", [shop]);

        var shopConnection = (await builder.ShopConnectionAdapter.GetAllAsync()).Single();
        Assert.Equal(string.Empty, shopConnection.ShopName);

        var shopEtsy = (await builder.ShopConnectionEtsyAdapter.GetAllAsync()).Single();
        Assert.Equal(string.Empty, shopEtsy.CountryCode);
        Assert.Equal(string.Empty, shopEtsy.ShopCurrency);
        Assert.Equal(string.Empty, shopEtsy.ShopUrl);
    }

    private static EtsyTokenResponse BuildTokens(string accessToken = "access-abc", string refreshToken = "refresh-xyz", int expiresIn = 3600)
    {
        return new EtsyTokenResponse
        {
            AccessToken = accessToken,
            ExpiresIn = expiresIn,
            RefreshToken = refreshToken,
            TokenType = "Bearer"
        };
    }

    private static EtsyShop BuildShop(long shopId = 65266817, string? shopName = "MyTalliTestShop", string? currency = "USD", string? country = "US", string? url = "https://www.etsy.com/shop/MyTalliTestShop", bool vacation = false)
    {
        return new EtsyShop
        {
            CountryIso = country,
            CurrencyCode = currency,
            IsVacation = vacation,
            ShopId = shopId,
            ShopName = shopName,
            Url = url
        };
    }

    #endregion
}
