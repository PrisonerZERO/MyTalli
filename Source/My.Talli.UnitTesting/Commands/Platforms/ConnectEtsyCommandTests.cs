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
        Assert.Equal("1226690893", connection.PlatformAccountId);
        Assert.Equal("Active", connection.ConnectionStatus);
        Assert.Equal("access-abc", connection.AccessToken);
        Assert.Equal("refresh-xyz", connection.RefreshToken);
    }

    [Fact]
    public async Task Execute_NewConnection_SetsTokenExpiryRelativeToNow()
    {
        var builder = new PlatformHandlerBuilder();
        var tokens = BuildTokens(expiresIn: 3600);

        var before = DateTime.UtcNow;
        await builder.Command.ExecuteAsync(1, tokens, "user", [BuildShop()]);
        var after = DateTime.UtcNow;

        var connection = (await builder.PlatformConnectionAdapter.GetAllAsync()).Single();
        Assert.NotNull(connection.TokenExpiryDateTime);
        Assert.InRange(connection.TokenExpiryDateTime!.Value, before.AddSeconds(3600 - 2), after.AddSeconds(3600 + 2));
    }

    [Fact]
    public async Task Execute_ExistingConnection_UpdatesTokensInPlace()
    {
        var builder = new PlatformHandlerBuilder();
        await builder.Command.ExecuteAsync(1, BuildTokens("old-access", "old-refresh"), "user", [BuildShop()]);

        await builder.Command.ExecuteAsync(1, BuildTokens("new-access", "new-refresh"), "user", [BuildShop()]);

        var connections = await builder.PlatformConnectionAdapter.GetAllAsync();
        var connection = Assert.Single(connections);
        Assert.Equal("new-access", connection.AccessToken);
        Assert.Equal("new-refresh", connection.RefreshToken);
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
