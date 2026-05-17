namespace My.Talli.UnitTesting.Commands.Billing;

using My.Talli.UnitTesting.Infrastructure.Builders;

using FRAMEWORK = My.Talli.Domain.Framework;

/// <summary>Tests</summary>
public class CanConnectAnotherShopCommandTests
{
	#region <Methods>

	[Fact]
	public async Task ExecuteAsync_FreeTier_NoShops_ReturnsTrue()
	{
		var builder = new BillingHealthBuilder();

		var result = await builder.CanConnectAnotherShop.ExecuteAsync(userId: 1);

		Assert.True(result);
	}

	[Fact]
	public async Task ExecuteAsync_FreeTier_OneShop_ReturnsFalse()
	{
		var builder = new BillingHealthBuilder();
		await builder.SeedShopAsync(userId: 1, platform: "Etsy");

		var result = await builder.CanConnectAnotherShop.ExecuteAsync(userId: 1);

		Assert.False(result);
	}

	[Fact]
	public async Task ExecuteAsync_FreeTier_OneShopOnDifferentPlatform_ReturnsFalse()
	{
		// Confirms the gate counts shops across all platforms, not per-platform.
		var builder = new BillingHealthBuilder();
		await builder.SeedShopAsync(userId: 1, platform: "Etsy");

		// Now the user is trying to connect Gumroad — gate should still block because total shop count is 1.
		var result = await builder.CanConnectAnotherShop.ExecuteAsync(userId: 1);

		Assert.False(result);
	}

	[Fact]
	public async Task ExecuteAsync_Pro_NoShops_ReturnsTrue()
	{
		var builder = new BillingHealthBuilder();
		await builder.SeedActiveProAsync(userId: 1, stripeSubscriptionId: "sub_pro", endDate: DateTime.UtcNow.AddDays(15));

		var result = await builder.CanConnectAnotherShop.ExecuteAsync(userId: 1);

		Assert.True(result);
	}

	[Fact]
	public async Task ExecuteAsync_Pro_MultipleShops_ReturnsTrue()
	{
		var builder = new BillingHealthBuilder();
		await builder.SeedActiveProAsync(userId: 1, stripeSubscriptionId: "sub_pro", endDate: DateTime.UtcNow.AddDays(15));
		await builder.SeedShopAsync(userId: 1, platform: "Etsy");
		await builder.SeedShopAsync(userId: 1, platform: "Gumroad");
		await builder.SeedShopAsync(userId: 1, platform: "Stripe");

		var result = await builder.CanConnectAnotherShop.ExecuteAsync(userId: 1);

		Assert.True(result);
	}

	[Fact]
	public async Task ExecuteAsync_ProCancelling_WithShop_ReturnsTrue()
	{
		// Cancelling = scheduled to cancel but still inside paid period.
		var builder = new BillingHealthBuilder();
		await builder.SeedActiveProAsync(userId: 1, stripeSubscriptionId: "sub_pro", endDate: DateTime.UtcNow.AddDays(20), status: FRAMEWORK.SubscriptionStatuses.Cancelling);
		await builder.SeedShopAsync(userId: 1, platform: "Etsy");

		var result = await builder.CanConnectAnotherShop.ExecuteAsync(userId: 1);

		Assert.True(result);
	}

	[Fact]
	public async Task ExecuteAsync_ProActiveButEndDateExpired_TreatedAsFree_ReturnsFalseWhenShopsExist()
	{
		// The whole point of the EndDate belt-and-suspenders gate: an Active sub with EndDate in the past should NOT grant Pro privileges.
		var builder = new BillingHealthBuilder();
		await builder.SeedActiveProAsync(userId: 1, stripeSubscriptionId: "sub_pro_expired", endDate: DateTime.UtcNow.AddDays(-5));
		await builder.SeedShopAsync(userId: 1, platform: "Etsy");

		var result = await builder.CanConnectAnotherShop.ExecuteAsync(userId: 1);

		Assert.False(result);
	}

	[Fact]
	public async Task ExecuteAsync_ProActiveButEndDateExpired_NoShops_ReturnsTrue()
	{
		// Even an expired Pro sub doesn't block someone who's at zero shops — they're just a free user with 0 shops.
		var builder = new BillingHealthBuilder();
		await builder.SeedActiveProAsync(userId: 1, stripeSubscriptionId: "sub_pro_expired", endDate: DateTime.UtcNow.AddDays(-5));

		var result = await builder.CanConnectAnotherShop.ExecuteAsync(userId: 1);

		Assert.True(result);
	}

	[Fact]
	public async Task ExecuteAsync_OtherUsersShopsDoNotCount()
	{
		// User 99 has 3 shops; user 1 has 0. User 1 should still be allowed to connect.
		var builder = new BillingHealthBuilder();
		await builder.SeedShopAsync(userId: 99, platform: "Etsy");
		await builder.SeedShopAsync(userId: 99, platform: "Gumroad");
		await builder.SeedShopAsync(userId: 99, platform: "Stripe");

		var result = await builder.CanConnectAnotherShop.ExecuteAsync(userId: 1);

		Assert.True(result);
	}

	#endregion
}
