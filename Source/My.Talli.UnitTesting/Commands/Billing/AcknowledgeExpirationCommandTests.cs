namespace My.Talli.UnitTesting.Commands.Billing;

using My.Talli.UnitTesting.Infrastructure.Builders;

/// <summary>Tests</summary>
public class AcknowledgeExpirationCommandTests
{
	#region <Methods>

	[Fact]
	public async Task ExecuteAsync_OwnSubscription_StampsExpirationAcknowledgedAt()
	{
		var builder = new BillingHealthBuilder();
		var sub = await builder.SeedActiveProAsync(userId: 42, stripeSubscriptionId: "sub_test", endDate: DateTime.UtcNow.AddDays(-5));

		await builder.AcknowledgeExpiration.ExecuteAsync(sub.Id, userId: 42);

		var reloaded = await builder.SubscriptionAdapter.GetByIdAsync(sub.Id);
		Assert.NotNull(reloaded);
		Assert.NotNull(reloaded!.ExpirationAcknowledgedAt);
	}

	[Fact]
	public async Task ExecuteAsync_DifferentUser_DoesNotStamp()
	{
		var builder = new BillingHealthBuilder();
		var sub = await builder.SeedActiveProAsync(userId: 42, stripeSubscriptionId: "sub_test", endDate: DateTime.UtcNow.AddDays(-5));

		await builder.AcknowledgeExpiration.ExecuteAsync(sub.Id, userId: 99);

		var reloaded = await builder.SubscriptionAdapter.GetByIdAsync(sub.Id);
		Assert.NotNull(reloaded);
		Assert.Null(reloaded!.ExpirationAcknowledgedAt);
	}

	[Fact]
	public async Task ExecuteAsync_UnknownSubscriptionId_NoOps()
	{
		var builder = new BillingHealthBuilder();

		await builder.AcknowledgeExpiration.ExecuteAsync(subscriptionId: 12345, userId: 42);

		// No exception, no rows created
		Assert.Empty(await builder.SubscriptionAdapter.GetAllAsync());
	}

	[Fact]
	public async Task ExecuteAsync_AlreadyAcknowledged_NoOps()
	{
		var builder = new BillingHealthBuilder();
		var sub = await builder.SeedActiveProAsync(userId: 42, stripeSubscriptionId: "sub_test", endDate: DateTime.UtcNow.AddDays(-5));

		await builder.AcknowledgeExpiration.ExecuteAsync(sub.Id, userId: 42);
		var firstAck = (await builder.SubscriptionAdapter.GetByIdAsync(sub.Id))!.ExpirationAcknowledgedAt;

		await Task.Delay(15);
		await builder.AcknowledgeExpiration.ExecuteAsync(sub.Id, userId: 42);
		var secondAck = (await builder.SubscriptionAdapter.GetByIdAsync(sub.Id))!.ExpirationAcknowledgedAt;

		Assert.Equal(firstAck, secondAck);
	}

	#endregion
}
