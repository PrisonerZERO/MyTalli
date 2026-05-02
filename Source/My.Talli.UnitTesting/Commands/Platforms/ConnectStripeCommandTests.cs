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

        await builder.StripeCommand.ExecuteAsync(userId: 42, BuildAccount());

        var connection = Assert.Single(await builder.PlatformConnectionAdapter.GetAllAsync());
        Assert.Equal(42, connection.UserId);
        Assert.Equal("Stripe", connection.Platform);
        Assert.Equal("Active", connection.ConnectionStatus);
    }

    [Fact]
    public async Task Execute_NewConnection_LeavesAccessTokenEmpty()
    {
        var builder = new PlatformHandlerBuilder();

        await builder.StripeCommand.ExecuteAsync(42, BuildAccount());

        var shop = Assert.Single(await builder.ShopConnectionAdapter.GetAllAsync());
        Assert.Equal(string.Empty, shop.AccessToken);
    }

    [Fact]
    public async Task Execute_NewConnection_LeavesRefreshTokenAndExpiriesNull()
    {
        var builder = new PlatformHandlerBuilder();

        await builder.StripeCommand.ExecuteAsync(1, BuildAccount());

        var shop = Assert.Single(await builder.ShopConnectionAdapter.GetAllAsync());
        Assert.Null(shop.RefreshToken);
        Assert.Null(shop.RefreshTokenExpiryDateTime);
        Assert.Null(shop.TokenExpiryDateTime);
    }

    [Fact]
    public async Task Execute_NewConnection_StoresAccountIdInBothShopColumns()
    {
        var builder = new PlatformHandlerBuilder();
        var account = BuildAccount(accountId: "acct_test_123", businessName: "Robert's Shop");

        await builder.StripeCommand.ExecuteAsync(42, account);

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

        await builder.StripeCommand.ExecuteAsync(1, account);

        var shop = Assert.Single(await builder.ShopConnectionAdapter.GetAllAsync());
        Assert.Equal("creator@example.com", shop.ShopName);
    }

    [Fact]
    public async Task Execute_NewConnection_FallsBackToStripeWhenBusinessNameAndEmailMissing()
    {
        var builder = new PlatformHandlerBuilder();
        var account = new StripeAccountInfo { AccountId = "acct_x", BusinessName = string.Empty, Email = null };

        await builder.StripeCommand.ExecuteAsync(1, account);

        var shop = Assert.Single(await builder.ShopConnectionAdapter.GetAllAsync());
        Assert.Equal("Stripe", shop.ShopName);
    }

    [Fact]
    public async Task Execute_NewConnection_NextSyncIsSetToNow()
    {
        var builder = new PlatformHandlerBuilder();

        var before = DateTime.UtcNow;
        await builder.StripeCommand.ExecuteAsync(1, BuildAccount());
        var after = DateTime.UtcNow;

        var shop = Assert.Single(await builder.ShopConnectionAdapter.GetAllAsync());
        Assert.InRange(shop.NextSyncDateTime, before.AddSeconds(-2), after.AddSeconds(2));
    }

    [Fact]
    public async Task Execute_FirstConnection_ReportsFirstConnectionAndNewShop()
    {
        var builder = new PlatformHandlerBuilder();

        var result = await builder.StripeCommand.ExecuteAsync(1, BuildAccount());

        Assert.True(result.IsFirstConnection);
        Assert.True(result.WasNewShop);
    }

    [Fact]
    public async Task Execute_Reconnect_SameAccount_ReportsRefreshOnly()
    {
        var builder = new PlatformHandlerBuilder();
        await builder.StripeCommand.ExecuteAsync(1, BuildAccount(accountId: "acct_1"));

        var result = await builder.StripeCommand.ExecuteAsync(1, BuildAccount(accountId: "acct_1"));

        Assert.False(result.IsFirstConnection);
        Assert.False(result.WasNewShop);
        Assert.Single(await builder.ShopConnectionAdapter.GetAllAsync());
    }

    [Fact]
    public async Task Execute_DifferentStripeAccount_ReportsAddedShop()
    {
        var builder = new PlatformHandlerBuilder();
        await builder.StripeCommand.ExecuteAsync(1, BuildAccount(accountId: "acct_a"));

        var result = await builder.StripeCommand.ExecuteAsync(1, BuildAccount(accountId: "acct_b"));

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
