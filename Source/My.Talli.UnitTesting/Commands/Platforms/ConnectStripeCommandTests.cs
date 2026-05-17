namespace My.Talli.UnitTesting.Commands.Platforms;

using Domain.Components;
using My.Talli.UnitTesting.Infrastructure.Builders;

/// <summary>Tests</summary>
public class ConnectStripeCommandTests
{
    #region <Methods>

    [Fact]
    public async Task Execute_NewConnection_InsertsPlatformConnection()
    {
        var builder = new PlatformHandlerBuilder();

        await builder.StripeCommand.ExecuteAsync(userId: 42, BuildAccount(), "sk_test_oauth_001", "rt_001");

        var connection = Assert.Single(await builder.PlatformConnectionAdapter.GetAllAsync());
        Assert.Equal(42, connection.UserId);
        Assert.Equal("Stripe", connection.Platform);
        Assert.Equal("Active", connection.ConnectionStatus);
    }

    [Fact]
    public async Task Execute_NewConnection_StoresAccessTokenAndRefreshToken()
    {
        var builder = new PlatformHandlerBuilder();

        await builder.StripeCommand.ExecuteAsync(42, BuildAccount(), "sk_test_oauth_abc", "rt_xyz");

        var shop = Assert.Single(await builder.ShopConnectionAdapter.GetAllAsync());
        Assert.Equal("sk_test_oauth_abc", shop.AccessToken);
        Assert.Equal("rt_xyz", shop.RefreshToken);
    }

    [Fact]
    public async Task Execute_NewConnection_NullRefreshToken_IsStored()
    {
        var builder = new PlatformHandlerBuilder();

        await builder.StripeCommand.ExecuteAsync(1, BuildAccount(), "sk_test_oauth", refreshToken: null);

        var shop = Assert.Single(await builder.ShopConnectionAdapter.GetAllAsync());
        Assert.Null(shop.RefreshToken);
    }

    [Fact]
    public async Task Execute_NewConnection_LeavesTokenExpiriesNull()
    {
        var builder = new PlatformHandlerBuilder();

        await builder.StripeCommand.ExecuteAsync(1, BuildAccount(), "sk_test_oauth", "rt");

        var shop = Assert.Single(await builder.ShopConnectionAdapter.GetAllAsync());
        Assert.Null(shop.RefreshTokenExpiryDateTime);
        Assert.Null(shop.TokenExpiryDateTime);
    }

    [Fact]
    public async Task Execute_NewConnection_StoresAccountIdInBothShopColumns()
    {
        var builder = new PlatformHandlerBuilder();
        var account = BuildAccount(accountId: "acct_test_123", businessName: "Robert's Shop");

        await builder.StripeCommand.ExecuteAsync(42, account, "sk_test_oauth", "rt");

        var shop = Assert.Single(await builder.ShopConnectionAdapter.GetAllAsync());
        Assert.Equal("acct_test_123", shop.PlatformShopId);
        Assert.Equal("acct_test_123", shop.PlatformAccountId);
        Assert.Equal("Robert's Shop", shop.ShopName);
        Assert.Equal("Pending", shop.Status);
        Assert.True(shop.IsActive);
        Assert.True(shop.IsEnabled);
        Assert.Equal(42, shop.UserId);
    }

    [Fact]
    public async Task Execute_NewConnection_FallsBackToEmailWhenBusinessNameMissing()
    {
        var builder = new PlatformHandlerBuilder();
        var account = new StripeAccountInfo { AccountId = "acct_x", BusinessName = string.Empty, Email = "creator@example.com" };

        await builder.StripeCommand.ExecuteAsync(1, account, "sk_test_oauth", "rt");

        var shop = Assert.Single(await builder.ShopConnectionAdapter.GetAllAsync());
        Assert.Equal("creator@example.com", shop.ShopName);
    }

    [Fact]
    public async Task Execute_NewConnection_FallsBackToStripeWhenBusinessNameAndEmailMissing()
    {
        var builder = new PlatformHandlerBuilder();
        var account = new StripeAccountInfo { AccountId = "acct_x", BusinessName = string.Empty, Email = null };

        await builder.StripeCommand.ExecuteAsync(1, account, "sk_test_oauth", "rt");

        var shop = Assert.Single(await builder.ShopConnectionAdapter.GetAllAsync());
        Assert.Equal("Stripe", shop.ShopName);
    }

    [Fact]
    public async Task Execute_NewConnection_NextSyncIsSetToNow()
    {
        var builder = new PlatformHandlerBuilder();

        var before = DateTime.UtcNow;
        await builder.StripeCommand.ExecuteAsync(1, BuildAccount(), "sk_test_oauth", "rt");
        var after = DateTime.UtcNow;

        var shop = Assert.Single(await builder.ShopConnectionAdapter.GetAllAsync());
        Assert.InRange(shop.NextSyncDateTime, before.AddSeconds(-2), after.AddSeconds(2));
    }

    [Fact]
    public async Task Execute_FirstConnection_ReportsFirstConnectionAndNewShop()
    {
        var builder = new PlatformHandlerBuilder();

        var result = await builder.StripeCommand.ExecuteAsync(1, BuildAccount(), "sk_test_oauth", "rt");

        Assert.True(result.IsFirstConnection);
        Assert.True(result.WasNewShop);
    }

    [Fact]
    public async Task Execute_Reconnect_SameAccount_ReportsRefreshOnly_AndUpdatesAccessToken()
    {
        var builder = new PlatformHandlerBuilder();
        await builder.StripeCommand.ExecuteAsync(1, BuildAccount(accountId: "acct_1"), "sk_test_oauth_old", "rt_old");

        var result = await builder.StripeCommand.ExecuteAsync(1, BuildAccount(accountId: "acct_1"), "sk_test_oauth_new", "rt_new");

        Assert.False(result.IsFirstConnection);
        Assert.False(result.WasNewShop);
        var shop = Assert.Single(await builder.ShopConnectionAdapter.GetAllAsync());
        Assert.Equal("sk_test_oauth_new", shop.AccessToken);
        Assert.Equal("rt_new", shop.RefreshToken);
    }

    [Fact]
    public async Task Execute_DifferentStripeAccount_ReportsAddedShop()
    {
        var builder = new PlatformHandlerBuilder();
        await builder.StripeCommand.ExecuteAsync(1, BuildAccount(accountId: "acct_a"), "sk_test_oauth_a", "rt_a");

        var result = await builder.StripeCommand.ExecuteAsync(1, BuildAccount(accountId: "acct_b"), "sk_test_oauth_b", "rt_b");

        Assert.False(result.IsFirstConnection);
        Assert.True(result.WasNewShop);
        Assert.Equal(2, (await builder.ShopConnectionAdapter.GetAllAsync()).Count());
    }

    private static StripeAccountInfo BuildAccount(string accountId = "acct_test_001", string businessName = "MyTalli Test Shop")
    {
        return new StripeAccountInfo
        {
            AccountId = accountId,
            BusinessName = businessName,
            Email = "creator@example.com"
        };
    }

    #endregion
}
