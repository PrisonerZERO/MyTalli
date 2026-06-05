namespace My.Talli.UnitTesting.Commands.Platforms;

using Domain.Commands.Platforms;
using Domain.Models;
using My.Talli.UnitTesting.Infrastructure.Builders;

/// <summary>Tests</summary>
public class SwapShopAccountCommandTests
{
    #region <Methods>

    [Fact]
    public async Task Execute_DifferentAccount_RepointsShopAndResetsSyncState()
    {
        var builder = new PlatformHandlerBuilder();
        var shopId = await SeedShopAsync(builder, userId: 1, platform: "Stripe", platformShopId: "acct_old");

        var result = await builder.SwapShopAccount.ExecuteAsync(BuildInput(shopId, userId: 1, platform: "Stripe", platformShopId: "acct_new"));

        Assert.True(result.Found);
        Assert.True(result.Swapped);

        var shop = Assert.Single(await builder.ShopConnectionAdapter.GetAllAsync());
        Assert.Equal("acct_new", shop.PlatformShopId);
        Assert.Equal("acct_new", shop.PlatformAccountId);
        Assert.Equal("token_new", shop.AccessToken);
        Assert.Equal("New Shop", shop.ShopName);
        Assert.Equal("Pending", shop.Status);
        Assert.Null(shop.LastSyncDateTime);
        Assert.Equal(0, shop.ConsecutiveFailures);
        Assert.Null(shop.LastErrorMessage);
    }

    [Fact]
    public async Task Execute_DifferentAccount_DeletesShopRevenueExpensePayout()
    {
        var builder = new PlatformHandlerBuilder();
        var shopId = await SeedShopAsync(builder, userId: 1, platform: "Stripe", platformShopId: "acct_old");
        await SeedDataAsync(builder, userId: 1, shopId);

        await builder.SwapShopAccount.ExecuteAsync(BuildInput(shopId, userId: 1, platform: "Stripe", platformShopId: "acct_new"));

        Assert.Empty(await builder.RevenueAdapter.FindAsync(r => r.ShopConnectionId == shopId));
        Assert.Empty(await builder.ExpenseAdapter.FindAsync(e => e.ShopConnectionId == shopId));
        Assert.Empty(await builder.PayoutAdapter.FindAsync(p => p.ShopConnectionId == shopId));
    }

    [Fact]
    public async Task Execute_SameAccount_RefreshesTokensWithoutWipingData()
    {
        var builder = new PlatformHandlerBuilder();
        var shopId = await SeedShopAsync(builder, userId: 1, platform: "Stripe", platformShopId: "acct_same");
        await SeedDataAsync(builder, userId: 1, shopId);

        var result = await builder.SwapShopAccount.ExecuteAsync(BuildInput(shopId, userId: 1, platform: "Stripe", platformShopId: "acct_same"));

        Assert.True(result.Found);
        Assert.False(result.Swapped);

        var shop = Assert.Single(await builder.ShopConnectionAdapter.GetAllAsync());
        Assert.Equal("token_new", shop.AccessToken);
        Assert.NotNull(shop.LastSyncDateTime);
        Assert.Equal(2, (await builder.RevenueAdapter.FindAsync(r => r.ShopConnectionId == shopId)).Count());
        Assert.NotEmpty(await builder.ExpenseAdapter.FindAsync(e => e.ShopConnectionId == shopId));
        Assert.NotEmpty(await builder.PayoutAdapter.FindAsync(p => p.ShopConnectionId == shopId));
    }

    [Fact]
    public async Task Execute_DifferentUser_DoesNotSwapOrWipe()
    {
        var builder = new PlatformHandlerBuilder();
        var shopId = await SeedShopAsync(builder, userId: 1, platform: "Stripe", platformShopId: "acct_old");
        await SeedDataAsync(builder, userId: 1, shopId);

        var result = await builder.SwapShopAccount.ExecuteAsync(BuildInput(shopId, userId: 999, platform: "Stripe", platformShopId: "acct_new"));

        Assert.False(result.Found);
        Assert.False(result.Swapped);

        var shop = Assert.Single(await builder.ShopConnectionAdapter.GetAllAsync());
        Assert.Equal("acct_old", shop.PlatformShopId);
        Assert.NotEmpty(await builder.RevenueAdapter.FindAsync(r => r.ShopConnectionId == shopId));
    }

    [Fact]
    public async Task Execute_PlatformMismatch_DoesNotSwap()
    {
        var builder = new PlatformHandlerBuilder();
        var shopId = await SeedShopAsync(builder, userId: 1, platform: "Stripe", platformShopId: "acct_old");

        var result = await builder.SwapShopAccount.ExecuteAsync(BuildInput(shopId, userId: 1, platform: "Etsy", platformShopId: "acct_new"));

        Assert.False(result.Found);

        var shop = Assert.Single(await builder.ShopConnectionAdapter.GetAllAsync());
        Assert.Equal("acct_old", shop.PlatformShopId);
    }

    [Fact]
    public async Task Execute_EtsySwap_UpsertsShopConnectionEtsyDetail()
    {
        var builder = new PlatformHandlerBuilder();
        var shopId = await SeedShopAsync(builder, userId: 1, platform: "Etsy", platformShopId: "111");

        var input = BuildInput(shopId, userId: 1, platform: "Etsy", platformShopId: "222");
        input.EtsyDetail = new SwapShopEtsyDetail { CountryCode = "GB", IsVacationMode = true, ShopCurrency = "GBP", ShopUrl = "https://etsy.com/shop/new" };
        await builder.SwapShopAccount.ExecuteAsync(input);

        var detail = Assert.Single(await builder.ShopConnectionEtsyAdapter.GetAllAsync());
        Assert.Equal(shopId, detail.Id);
        Assert.Equal("GB", detail.CountryCode);
        Assert.True(detail.IsVacationMode);
        Assert.Equal("GBP", detail.ShopCurrency);
        Assert.Equal("https://etsy.com/shop/new", detail.ShopUrl);
    }

    private static SwapShopAccountInput BuildInput(long shopConnectionId, long userId, string platform, string platformShopId)
    {
        return new SwapShopAccountInput
        {
            AccessToken = "token_new",
            Platform = platform,
            PlatformAccountId = platformShopId,
            PlatformShopId = platformShopId,
            RefreshToken = "rt_new",
            ShopConnectionId = shopConnectionId,
            ShopName = "New Shop",
            UserId = userId
        };
    }

    private static async Task<long> SeedShopAsync(PlatformHandlerBuilder builder, long userId, string platform, string platformShopId)
    {
        var connection = await builder.PlatformConnectionAdapter.InsertAsync(new PlatformConnection
        {
            ConnectionStatus = "Active",
            Platform = platform,
            UserId = userId
        });

        var shop = await builder.ShopConnectionAdapter.InsertAsync(new ShopConnection
        {
            AccessToken = "token_old",
            ConsecutiveFailures = 3,
            IsActive = true,
            IsEnabled = true,
            LastErrorMessage = "previous error",
            LastSyncDateTime = DateTime.UtcNow.AddHours(-2),
            NextSyncDateTime = DateTime.UtcNow,
            PlatformAccountId = platformShopId,
            PlatformConnectionId = connection.Id,
            PlatformShopId = platformShopId,
            RefreshToken = "rt_old",
            ShopName = "Old Shop",
            Status = "Completed",
            UserId = userId
        });

        return shop.Id;
    }

    private static async Task SeedDataAsync(PlatformHandlerBuilder builder, long userId, long shopConnectionId)
    {
        await builder.RevenueAdapter.InsertAsync(new Revenue { Currency = "USD", Platform = "Stripe", PlatformTransactionId = "txn_1", ShopConnectionId = shopConnectionId, UserId = userId });
        await builder.RevenueAdapter.InsertAsync(new Revenue { Currency = "USD", Platform = "Stripe", PlatformTransactionId = "txn_2", ShopConnectionId = shopConnectionId, UserId = userId });
        await builder.ExpenseAdapter.InsertAsync(new Expense { Currency = "USD", Platform = "Stripe", ShopConnectionId = shopConnectionId, UserId = userId });
        await builder.PayoutAdapter.InsertAsync(new Payout { Currency = "USD", Platform = "Stripe", ShopConnectionId = shopConnectionId, UserId = userId });
    }

    #endregion
}
