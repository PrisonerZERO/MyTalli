namespace My.Talli.UnitTesting.Commands.Billing;

using Domain.Models;
using My.Talli.UnitTesting.Infrastructure.Builders;

using FRAMEWORK = My.Talli.Domain.Framework;

/// <summary>Tests</summary>
public class FindExpiredUnacknowledgedSubscriptionCommandTests
{
	#region <Methods>

	[Fact]
	public async Task ExecuteAsync_NoSubscriptions_ReturnsNull()
	{
		var builder = new BillingHealthBuilder();

		var result = await builder.FindExpiredUnacknowledged.ExecuteAsync(userId: 42);

		Assert.Null(result);
	}

	[Fact]
	public async Task ExecuteAsync_ExpiredUnacknowledgedPro_ReturnsIt()
	{
		var builder = new BillingHealthBuilder();
		var sub = await builder.SeedActiveProAsync(userId: 42, stripeSubscriptionId: "sub_test", endDate: DateTime.UtcNow.AddDays(-3), status: FRAMEWORK.SubscriptionStatuses.Cancelling);

		var result = await builder.FindExpiredUnacknowledged.ExecuteAsync(userId: 42);

		Assert.NotNull(result);
		Assert.Equal(sub.Id, result!.Id);
	}

	[Fact]
	public async Task ExecuteAsync_AcknowledgedSubscription_ReturnsNull()
	{
		var builder = new BillingHealthBuilder();
		var sub = await builder.SeedActiveProAsync(userId: 42, stripeSubscriptionId: "sub_test", endDate: DateTime.UtcNow.AddDays(-3));
		sub.ExpirationAcknowledgedAt = DateTime.UtcNow.AddHours(-1);
		await builder.SubscriptionAdapter.UpdateAsync(sub);

		var result = await builder.FindExpiredUnacknowledged.ExecuteAsync(userId: 42);

		Assert.Null(result);
	}

	[Fact]
	public async Task ExecuteAsync_NotYetExpired_ReturnsNull()
	{
		var builder = new BillingHealthBuilder();
		await builder.SeedActiveProAsync(userId: 42, stripeSubscriptionId: "sub_test", endDate: DateTime.UtcNow.AddDays(7));

		var result = await builder.FindExpiredUnacknowledged.ExecuteAsync(userId: 42);

		Assert.Null(result);
	}

	[Fact]
	public async Task ExecuteAsync_DifferentUserHasExpired_ReturnsNullForRequestedUser()
	{
		var builder = new BillingHealthBuilder();
		await builder.SeedActiveProAsync(userId: 99, stripeSubscriptionId: "sub_test", endDate: DateTime.UtcNow.AddDays(-5));

		var result = await builder.FindExpiredUnacknowledged.ExecuteAsync(userId: 42);

		Assert.Null(result);
	}

	[Fact]
	public async Task ExecuteAsync_NonProProduct_ReturnsNull()
	{
		var builder = new BillingHealthBuilder();
		await builder.SubscriptionAdapter.InsertAsync(new Subscription
		{
			EndDate = DateTime.UtcNow.AddDays(-5),
			OrderItemId = 0,
			ProductId = 99,
			RenewalDate = DateTime.UtcNow.AddDays(-5),
			StartDate = DateTime.UtcNow.AddMonths(-2),
			Status = FRAMEWORK.SubscriptionStatuses.Active,
			UserId = 42
		});

		var result = await builder.FindExpiredUnacknowledged.ExecuteAsync(userId: 42);

		Assert.Null(result);
	}

	[Fact]
	public async Task ExecuteAsync_MultipleExpiredUnacked_ReturnsMostRecent()
	{
		var builder = new BillingHealthBuilder();
		await builder.SeedActiveProAsync(userId: 42, stripeSubscriptionId: "sub_older", endDate: DateTime.UtcNow.AddDays(-60));
		var newer = await builder.SeedActiveProAsync(userId: 42, stripeSubscriptionId: "sub_newer", endDate: DateTime.UtcNow.AddDays(-3));

		var result = await builder.FindExpiredUnacknowledged.ExecuteAsync(userId: 42);

		Assert.NotNull(result);
		Assert.Equal(newer.Id, result!.Id);
	}

	#endregion
}
