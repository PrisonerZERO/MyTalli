namespace My.Talli.UnitTesting.Commands.Billing;

using Domain.Models;
using My.Talli.UnitTesting.Infrastructure.Builders;

using FRAMEWORK = My.Talli.Domain.Framework;

/// <summary>Tests</summary>
public class IsProSubscriberCommandTests
{
	#region <Methods>

	[Fact]
	public async Task ExecuteAsync_NoSubscription_ReturnsFalse()
	{
		var builder = new BillingHealthBuilder();

		var result = await builder.IsProSubscriber.ExecuteAsync(userId: 1);

		Assert.False(result);
	}

	[Fact]
	public async Task ExecuteAsync_ActivePro_ReturnsTrue()
	{
		var builder = new BillingHealthBuilder();
		await builder.SeedActiveProAsync(userId: 1, stripeSubscriptionId: "sub_pro", endDate: DateTime.UtcNow.AddDays(15));

		var result = await builder.IsProSubscriber.ExecuteAsync(userId: 1);

		Assert.True(result);
	}

	[Fact]
	public async Task ExecuteAsync_CancellingPro_StillReturnsTrue()
	{
		var builder = new BillingHealthBuilder();
		await builder.SeedActiveProAsync(userId: 1, stripeSubscriptionId: "sub_pro", endDate: DateTime.UtcNow.AddDays(20), status: FRAMEWORK.SubscriptionStatuses.Cancelling);

		var result = await builder.IsProSubscriber.ExecuteAsync(userId: 1);

		Assert.True(result);
	}

	[Fact]
	public async Task ExecuteAsync_ProActiveButEndDateExpired_ReturnsFalse()
	{
		// The whole point of the EndDate gate — expired Pro is treated as Free.
		var builder = new BillingHealthBuilder();
		await builder.SeedActiveProAsync(userId: 1, stripeSubscriptionId: "sub_pro_expired", endDate: DateTime.UtcNow.AddDays(-5));

		var result = await builder.IsProSubscriber.ExecuteAsync(userId: 1);

		Assert.False(result);
	}

	[Fact]
	public async Task ExecuteAsync_CancelledStatus_ReturnsFalse()
	{
		var builder = new BillingHealthBuilder();
		await builder.SeedActiveProAsync(userId: 1, stripeSubscriptionId: "sub_pro_cancelled", endDate: DateTime.UtcNow.AddDays(15), status: FRAMEWORK.SubscriptionStatuses.Cancelled);

		var result = await builder.IsProSubscriber.ExecuteAsync(userId: 1);

		Assert.False(result);
	}

	[Fact]
	public async Task ExecuteAsync_NonProProduct_ReturnsFalse()
	{
		// A user subscribed only to a module (ProductId 99) is not Pro.
		var builder = new BillingHealthBuilder();
		await builder.SubscriptionAdapter.InsertAsync(new Subscription
		{
			EndDate = DateTime.UtcNow.AddDays(15),
			OrderItemId = 0,
			ProductId = 99,
			RenewalDate = DateTime.UtcNow.AddDays(15),
			StartDate = DateTime.UtcNow.AddMonths(-1),
			Status = FRAMEWORK.SubscriptionStatuses.Active,
			UserId = 1
		});

		var result = await builder.IsProSubscriber.ExecuteAsync(userId: 1);

		Assert.False(result);
	}

	[Fact]
	public async Task ExecuteAsync_OtherUsersPro_DoesNotAffectResult()
	{
		var builder = new BillingHealthBuilder();
		await builder.SeedActiveProAsync(userId: 99, stripeSubscriptionId: "sub_pro", endDate: DateTime.UtcNow.AddDays(15));

		var result = await builder.IsProSubscriber.ExecuteAsync(userId: 1);

		Assert.False(result);
	}

	#endregion
}
