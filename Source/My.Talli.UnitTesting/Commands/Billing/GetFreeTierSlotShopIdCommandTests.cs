namespace My.Talli.UnitTesting.Commands.Billing;

using My.Talli.UnitTesting.Infrastructure.Builders;

using FRAMEWORK = My.Talli.Domain.Framework;

/// <summary>Tests</summary>
public class GetFreeTierSlotShopIdCommandTests
{
	#region <Methods>

	[Fact]
	public async Task ExecuteAsync_ProUser_ReturnsNull()
	{
		// Pro users have no "free slot" to highlight — all shops are unlocked.
		var builder = new BillingHealthBuilder();
		await builder.SeedActiveProAsync(userId: 1, stripeSubscriptionId: "sub_pro", endDate: DateTime.UtcNow.AddDays(15));
		await builder.SeedShopAsync(userId: 1, platform: "Etsy");
		await builder.SeedShopAsync(userId: 1, platform: "Gumroad");

		var result = await builder.GetFreeTierSlotShopId.ExecuteAsync(userId: 1);

		Assert.Null(result);
	}

	[Fact]
	public async Task ExecuteAsync_FreeUser_NoShops_ReturnsNull()
	{
		var builder = new BillingHealthBuilder();

		var result = await builder.GetFreeTierSlotShopId.ExecuteAsync(userId: 1);

		Assert.Null(result);
	}

	[Fact]
	public async Task ExecuteAsync_FreeUser_OneShop_ReturnsThatShopId()
	{
		var builder = new BillingHealthBuilder();
		var shop = await builder.SeedShopAsync(userId: 1, platform: "Etsy");

		var result = await builder.GetFreeTierSlotShopId.ExecuteAsync(userId: 1);

		Assert.Equal(shop.Id, result);
	}

	[Fact]
	public async Task ExecuteAsync_FreeUser_MultipleShops_ReturnsOldestShopId()
	{
		// First-connected wins. Id is the proxy for age (IDENTITY is monotonic).
		var builder = new BillingHealthBuilder();
		var oldest = await builder.SeedShopAsync(userId: 1, platform: "Etsy");
		await builder.SeedShopAsync(userId: 1, platform: "Gumroad");
		await builder.SeedShopAsync(userId: 1, platform: "Stripe");

		var result = await builder.GetFreeTierSlotShopId.ExecuteAsync(userId: 1);

		Assert.Equal(oldest.Id, result);
	}

	[Fact]
	public async Task ExecuteAsync_ExpiredProUser_TreatedAsFree_ReturnsOldestShopId()
	{
		// The whole point of the EndDate gate: expired Pro = treated as free for capacity rules.
		var builder = new BillingHealthBuilder();
		await builder.SeedActiveProAsync(userId: 1, stripeSubscriptionId: "sub_pro_expired", endDate: DateTime.UtcNow.AddDays(-5));
		var oldest = await builder.SeedShopAsync(userId: 1, platform: "Etsy");
		await builder.SeedShopAsync(userId: 1, platform: "Gumroad");

		var result = await builder.GetFreeTierSlotShopId.ExecuteAsync(userId: 1);

		Assert.Equal(oldest.Id, result);
	}

	[Fact]
	public async Task ExecuteAsync_ProCancelling_StillReturnsNull()
	{
		// Cancelling = scheduled to cancel but still inside paid period.
		var builder = new BillingHealthBuilder();
		await builder.SeedActiveProAsync(userId: 1, stripeSubscriptionId: "sub_pro_cancelling", endDate: DateTime.UtcNow.AddDays(20), status: FRAMEWORK.SubscriptionStatuses.Cancelling);
		await builder.SeedShopAsync(userId: 1, platform: "Etsy");
		await builder.SeedShopAsync(userId: 1, platform: "Gumroad");

		var result = await builder.GetFreeTierSlotShopId.ExecuteAsync(userId: 1);

		Assert.Null(result);
	}

	[Fact]
	public async Task ExecuteAsync_OtherUsersShops_DoNotInfluenceResult()
	{
		var builder = new BillingHealthBuilder();
		await builder.SeedShopAsync(userId: 99, platform: "Etsy");
		await builder.SeedShopAsync(userId: 99, platform: "Gumroad");
		var myShop = await builder.SeedShopAsync(userId: 1, platform: "Stripe");

		var result = await builder.GetFreeTierSlotShopId.ExecuteAsync(userId: 1);

		Assert.Equal(myShop.Id, result);
	}

	#endregion
}
