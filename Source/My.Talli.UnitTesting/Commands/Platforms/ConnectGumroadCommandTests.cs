namespace My.Talli.UnitTesting.Commands.Platforms;

using Domain.Components;
using My.Talli.UnitTesting.Infrastructure.Builders;

/// <summary>Tests</summary>
public class ConnectGumroadCommandTests
{
    #region <Methods>

    [Fact]
    public async Task Execute_NewConnection_InsertsPlatformConnection()
    {
        var builder = new PlatformHandlerBuilder();

        await builder.GumroadCommand.ExecuteAsync(userId: 42, BuildTokens(), BuildUser());

        var connection = Assert.Single(await builder.PlatformConnectionAdapter.GetAllAsync());
        Assert.Equal(42, connection.UserId);
        Assert.Equal("Gumroad", connection.Platform);
        Assert.Equal("Active", connection.ConnectionStatus);
    }

    [Fact]
    public async Task Execute_NewConnection_StoresTokenOnShopConnection()
    {
        var builder = new PlatformHandlerBuilder();

        await builder.GumroadCommand.ExecuteAsync(42, BuildTokens(accessToken: "gum-access"), BuildUser());

        var shop = Assert.Single(await builder.ShopConnectionAdapter.GetAllAsync());
        Assert.Equal("gum-access", builder.TokenProtector.Unprotect(shop.AccessToken));
    }

    [Fact]
    public async Task Execute_NewConnection_StoredAccessTokenIsEncrypted()
    {
        var builder = new PlatformHandlerBuilder();

        await builder.GumroadCommand.ExecuteAsync(42, BuildTokens(accessToken: "plain-access"), BuildUser());

        var shop = Assert.Single(await builder.ShopConnectionAdapter.GetAllAsync());
        Assert.NotEqual("plain-access", shop.AccessToken);
    }

    [Fact]
    public async Task Execute_NewConnection_LeavesRefreshTokenAndExpiriesNull()
    {
        var builder = new PlatformHandlerBuilder();

        await builder.GumroadCommand.ExecuteAsync(1, BuildTokens(), BuildUser());

        var shop = Assert.Single(await builder.ShopConnectionAdapter.GetAllAsync());
        Assert.Null(shop.RefreshToken);
        Assert.Null(shop.RefreshTokenExpiryDateTime);
        Assert.Null(shop.TokenExpiryDateTime);
    }

    [Fact]
    public async Task Execute_NewConnection_PopulatesShopFromUser()
    {
        var builder = new PlatformHandlerBuilder();
        var user = BuildUser(userId: "user-xyz", name: "Robert Jordan");

        await builder.GumroadCommand.ExecuteAsync(42, BuildTokens(), user);

        var shop = Assert.Single(await builder.ShopConnectionAdapter.GetAllAsync());
        Assert.Equal("user-xyz", shop.PlatformShopId);
        Assert.Equal("user-xyz", shop.PlatformAccountId);
        Assert.Equal("Robert Jordan", shop.ShopName);
        Assert.Equal("Pending", shop.Status);
        Assert.True(shop.IsActive);
        Assert.True(shop.IsEnabled);
        Assert.Equal(42, shop.UserId);
    }

    [Fact]
    public async Task Execute_NewConnection_FallsBackToUserIdWhenUserIdFieldIsEmpty()
    {
        var builder = new PlatformHandlerBuilder();
        var user = new GumroadUser { Id = "raw-id", UserId = null, Name = "n" };

        await builder.GumroadCommand.ExecuteAsync(1, BuildTokens(), user);

        var shop = Assert.Single(await builder.ShopConnectionAdapter.GetAllAsync());
        Assert.Equal("raw-id", shop.PlatformShopId);
    }

    [Fact]
    public async Task Execute_NewConnection_FallsBackToEmailWhenNameMissing()
    {
        var builder = new PlatformHandlerBuilder();
        var user = new GumroadUser { Id = "u1", UserId = "u1", Name = null, Email = "creator@example.com" };

        await builder.GumroadCommand.ExecuteAsync(1, BuildTokens(), user);

        var shop = Assert.Single(await builder.ShopConnectionAdapter.GetAllAsync());
        Assert.Equal("creator@example.com", shop.ShopName);
    }

    [Fact]
    public async Task Execute_NewConnection_FallsBackToGumroadWhenNameAndEmailMissing()
    {
        var builder = new PlatformHandlerBuilder();
        var user = new GumroadUser { Id = "u1", UserId = "u1" };

        await builder.GumroadCommand.ExecuteAsync(1, BuildTokens(), user);

        var shop = Assert.Single(await builder.ShopConnectionAdapter.GetAllAsync());
        Assert.Equal("Gumroad", shop.ShopName);
    }

    [Fact]
    public async Task Execute_NewConnection_NextSyncIsSetToNow()
    {
        var builder = new PlatformHandlerBuilder();

        var before = DateTime.UtcNow;
        await builder.GumroadCommand.ExecuteAsync(1, BuildTokens(), BuildUser());
        var after = DateTime.UtcNow;

        var shop = Assert.Single(await builder.ShopConnectionAdapter.GetAllAsync());
        Assert.InRange(shop.NextSyncDateTime, before.AddSeconds(-2), after.AddSeconds(2));
    }

    [Fact]
    public async Task Execute_FirstConnection_ReportsFirstConnectionAndNewShop()
    {
        var builder = new PlatformHandlerBuilder();

        var result = await builder.GumroadCommand.ExecuteAsync(1, BuildTokens(), BuildUser());

        Assert.True(result.IsFirstConnection);
        Assert.True(result.WasNewShop);
    }

    [Fact]
    public async Task Execute_Reconnect_ReportsRefreshOnly()
    {
        var builder = new PlatformHandlerBuilder();
        await builder.GumroadCommand.ExecuteAsync(1, BuildTokens(accessToken: "old"), BuildUser());

        var result = await builder.GumroadCommand.ExecuteAsync(1, BuildTokens(accessToken: "new"), BuildUser());

        Assert.False(result.IsFirstConnection);
        Assert.False(result.WasNewShop);

        var shop = Assert.Single(await builder.ShopConnectionAdapter.GetAllAsync());
        Assert.Equal("new", builder.TokenProtector.Unprotect(shop.AccessToken));
    }

    [Fact]
    public async Task Execute_DifferentGumroadAccount_ReportsAddedShop()
    {
        var builder = new PlatformHandlerBuilder();
        await builder.GumroadCommand.ExecuteAsync(1, BuildTokens(), BuildUser(userId: "user-a"));

        var result = await builder.GumroadCommand.ExecuteAsync(1, BuildTokens(), BuildUser(userId: "user-b"));

        Assert.False(result.IsFirstConnection);
        Assert.True(result.WasNewShop);
        Assert.Equal(2, (await builder.ShopConnectionAdapter.GetAllAsync()).Count());
    }

    [Fact]
    public async Task Execute_ExistingShopWithFailures_ResetsConsecutiveFailuresAndClearsErrorMessage()
    {
        var builder = new PlatformHandlerBuilder();
        await builder.GumroadCommand.ExecuteAsync(1, BuildTokens(), BuildUser());

        // Simulate worker recording failures
        var stored = (await builder.ShopConnectionAdapter.GetAllAsync()).Single();
        stored.ConsecutiveFailures = 4;
        stored.LastErrorMessage = "HTTP 401 Unauthorized";
        stored.Status = "Failed";
        await builder.ShopConnectionAdapter.UpdateAsync(stored);

        // Reconnect
        await builder.GumroadCommand.ExecuteAsync(1, BuildTokens(), BuildUser());

        var refreshed = (await builder.ShopConnectionAdapter.GetAllAsync()).Single();
        Assert.Equal(0, refreshed.ConsecutiveFailures);
        Assert.Null(refreshed.LastErrorMessage);
        Assert.Equal("Pending", refreshed.Status);
    }

    private static GumroadTokenResponse BuildTokens(string accessToken = "gum-access")
    {
        return new GumroadTokenResponse
        {
            AccessToken = accessToken,
            Scope = "view_sales",
            TokenType = "Bearer"
        };
    }

    private static GumroadUser BuildUser(string userId = "user-xyz", string? name = "MyTalli Creator")
    {
        return new GumroadUser
        {
            Email = "creator@example.com",
            Id = userId,
            Name = name,
            UserId = userId,
            Url = "https://example.gumroad.com"
        };
    }

    #endregion
}
