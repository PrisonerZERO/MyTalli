namespace My.Talli.UnitTesting.Commands.Billing;

using Domain.Commands.Billing;
using My.Talli.UnitTesting.Infrastructure.Builders;

/// <summary>Tests</summary>
public class GetEarliestQueryableDateCommandTests
{
	#region <Methods>

	[Fact]
	public async Task ExecuteAsync_ProUser_ReturnsNull()
	{
		// Pro users have no history floor — full history.
		var builder = new BillingHealthBuilder();
		await builder.SeedActiveProAsync(userId: 1, stripeSubscriptionId: "sub_pro", endDate: DateTime.UtcNow.AddDays(15));

		var result = await builder.GetEarliestQueryableDate.ExecuteAsync(userId: 1);

		Assert.Null(result);
	}

	[Fact]
	public async Task ExecuteAsync_FreeUser_Returns30DayFloor()
	{
		var builder = new BillingHealthBuilder();

		var result = await builder.GetEarliestQueryableDate.ExecuteAsync(userId: 1);

		Assert.NotNull(result);
		var expected = DateTime.UtcNow.Date.AddDays(-GetEarliestQueryableDateCommand.FreeTierHistoryDays);
		Assert.Equal(expected, result!.Value);
	}

	[Fact]
	public async Task ExecuteAsync_ExpiredPro_TreatedAsFree_Returns30DayFloor()
	{
		var builder = new BillingHealthBuilder();
		await builder.SeedActiveProAsync(userId: 1, stripeSubscriptionId: "sub_pro_expired", endDate: DateTime.UtcNow.AddDays(-5));

		var result = await builder.GetEarliestQueryableDate.ExecuteAsync(userId: 1);

		Assert.NotNull(result);
		var expected = DateTime.UtcNow.Date.AddDays(-GetEarliestQueryableDateCommand.FreeTierHistoryDays);
		Assert.Equal(expected, result!.Value);
	}

	[Fact]
	public async Task ExecuteAsync_CancellingPro_StillReturnsNull()
	{
		var builder = new BillingHealthBuilder();
		await builder.SeedActiveProAsync(userId: 1, stripeSubscriptionId: "sub_pro_cancelling", endDate: DateTime.UtcNow.AddDays(20), status: My.Talli.Domain.Framework.SubscriptionStatuses.Cancelling);

		var result = await builder.GetEarliestQueryableDate.ExecuteAsync(userId: 1);

		Assert.Null(result);
	}

	#endregion
}
