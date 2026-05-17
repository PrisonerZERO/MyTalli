namespace My.Talli.UnitTesting.Workers;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using My.Talli.UnitTesting.Infrastructure.Builders;
using My.Talli.Web.Services.Billing;
using My.Talli.Web.Workers;

using FRAMEWORK = My.Talli.Domain.Framework;

/// <summary>Tests</summary>
public class BillingHealthWorkerTests
{
	#region <Methods>

	[Fact]
	public async Task RunPassAsync_NoActiveSubscriptions_WritesHeartbeatAndReturnsZeroDrift()
	{
		var builder = new BillingHealthBuilder();

		var drift = await BillingHealthWorker.RunPassAsync(builder.Container, NullLogger.Instance, TimeSpan.Zero, CancellationToken.None);

		Assert.Equal(0, drift);
		Assert.Empty(builder.ApiClient.Calls);

		var heartbeat = (await builder.HeartbeatAdapter.GetAllAsync()).Single();
		Assert.Equal(BillingHealthWorker.HeartbeatSourceName, heartbeat.HeartbeatSource);
		Assert.Equal(BillingHealthWorker.ExpectedIntervalSeconds, heartbeat.ExpectedIntervalSeconds);
	}

	[Fact]
	public async Task RunPassAsync_LocalAndStripeAligned_ReturnsZeroDriftAndCallsStripeOnce()
	{
		var builder = new BillingHealthBuilder();
		var endDate = DateTime.UtcNow.AddDays(15);
		await builder.SeedActiveProAsync(userId: 1, stripeSubscriptionId: "sub_aligned", endDate: endDate);
		builder.ApiClient.SetResponse("sub_aligned", new StripeSubscriptionInfo
		{
			CancelAtPeriodEnd = false,
			CurrentPeriodEnd = endDate,
			Status = "active",
			SubscriptionId = "sub_aligned"
		});

		var drift = await BillingHealthWorker.RunPassAsync(builder.Container, NullLogger.Instance, TimeSpan.Zero, CancellationToken.None);

		Assert.Equal(0, drift);
		Assert.Single(builder.ApiClient.Calls);
		Assert.Equal("sub_aligned", builder.ApiClient.Calls[0]);
		Assert.DoesNotContain(builder.ReconcileLogger.Entries, e => e.Level == LogLevel.Warning);
	}

	[Fact]
	public async Task RunPassAsync_StripeSaysCanceledButLocalActive_LogsDriftAndReturnsOne()
	{
		var builder = new BillingHealthBuilder();
		var endDate = DateTime.UtcNow.AddDays(15);
		await builder.SeedActiveProAsync(userId: 1, stripeSubscriptionId: "sub_canceled_on_stripe", endDate: endDate);
		builder.ApiClient.SetResponse("sub_canceled_on_stripe", new StripeSubscriptionInfo
		{
			CancelAtPeriodEnd = false,
			CurrentPeriodEnd = endDate,
			Status = "canceled",
			SubscriptionId = "sub_canceled_on_stripe"
		});

		var drift = await BillingHealthWorker.RunPassAsync(builder.Container, NullLogger.Instance, TimeSpan.Zero, CancellationToken.None);

		Assert.Equal(1, drift);
		Assert.Contains(builder.ReconcileLogger.Entries, e =>
			e.Level == LogLevel.Warning && e.Message.Contains("status mismatch"));
	}

	[Fact]
	public async Task RunPassAsync_StripeCancelAtPeriodEnd_MatchesLocalCancelling_NoDrift()
	{
		var builder = new BillingHealthBuilder();
		var endDate = DateTime.UtcNow.AddDays(20);
		await builder.SeedActiveProAsync(userId: 1, stripeSubscriptionId: "sub_cancelling", endDate: endDate, status: FRAMEWORK.SubscriptionStatuses.Cancelling);
		builder.ApiClient.SetResponse("sub_cancelling", new StripeSubscriptionInfo
		{
			CancelAtPeriodEnd = true,
			CurrentPeriodEnd = endDate,
			Status = "active",
			SubscriptionId = "sub_cancelling"
		});

		var drift = await BillingHealthWorker.RunPassAsync(builder.Container, NullLogger.Instance, TimeSpan.Zero, CancellationToken.None);

		Assert.Equal(0, drift);
		Assert.DoesNotContain(builder.ReconcileLogger.Entries, e => e.Level == LogLevel.Warning);
	}

	[Fact]
	public async Task RunPassAsync_StripeCurrentPeriodEndShiftedMoreThan1Hour_LogsDrift()
	{
		var builder = new BillingHealthBuilder();
		var localEnd = DateTime.UtcNow.AddDays(10);
		await builder.SeedActiveProAsync(userId: 1, stripeSubscriptionId: "sub_date_drift", endDate: localEnd);
		builder.ApiClient.SetResponse("sub_date_drift", new StripeSubscriptionInfo
		{
			CancelAtPeriodEnd = false,
			CurrentPeriodEnd = localEnd.AddDays(30),
			Status = "active",
			SubscriptionId = "sub_date_drift"
		});

		var drift = await BillingHealthWorker.RunPassAsync(builder.Container, NullLogger.Instance, TimeSpan.Zero, CancellationToken.None);

		Assert.Equal(1, drift);
		Assert.Contains(builder.ReconcileLogger.Entries, e =>
			e.Level == LogLevel.Warning && e.Message.Contains("EndDate mismatch"));
	}

	[Fact]
	public async Task RunPassAsync_StripeReturnsNotFound_LogsDrift()
	{
		var builder = new BillingHealthBuilder();
		var endDate = DateTime.UtcNow.AddDays(10);
		await builder.SeedActiveProAsync(userId: 1, stripeSubscriptionId: "sub_missing_on_stripe", endDate: endDate);
		builder.ApiClient.SetNotFound("sub_missing_on_stripe");

		var drift = await BillingHealthWorker.RunPassAsync(builder.Container, NullLogger.Instance, TimeSpan.Zero, CancellationToken.None);

		Assert.Equal(1, drift);
		Assert.Contains(builder.ReconcileLogger.Entries, e =>
			e.Level == LogLevel.Warning && e.Message.Contains("not found"));
	}

	[Fact]
	public async Task RunPassAsync_OrphanLocalSubscription_NoSubscriptionStripeRow_LogsDrift()
	{
		var builder = new BillingHealthBuilder();
		var endDate = DateTime.UtcNow.AddDays(10);
		await builder.SubscriptionAdapter.InsertAsync(new Domain.Models.Subscription
		{
			EndDate = endDate,
			OrderItemId = 0,
			ProductId = 1,
			RenewalDate = endDate,
			StartDate = endDate.AddMonths(-1),
			Status = FRAMEWORK.SubscriptionStatuses.Active,
			UserId = 1
		});

		var drift = await BillingHealthWorker.RunPassAsync(builder.Container, NullLogger.Instance, TimeSpan.Zero, CancellationToken.None);

		Assert.Equal(1, drift);
		Assert.Empty(builder.ApiClient.Calls);
		Assert.Contains(builder.ReconcileLogger.Entries, e =>
			e.Level == LogLevel.Warning && e.Message.Contains("no SubscriptionStripe row"));
	}

	[Fact]
	public async Task RunPassAsync_StripeThrows_ContinuesToNextSubAndLogsWarning()
	{
		var builder = new BillingHealthBuilder();
		var endDate = DateTime.UtcNow.AddDays(10);
		await builder.SeedActiveProAsync(userId: 1, stripeSubscriptionId: "sub_throws", endDate: endDate);
		await builder.SeedActiveProAsync(userId: 2, stripeSubscriptionId: "sub_ok", endDate: endDate);

		builder.ApiClient.SetThrows("sub_throws", new InvalidOperationException("Stripe boom"));
		builder.ApiClient.SetResponse("sub_ok", new StripeSubscriptionInfo
		{
			CancelAtPeriodEnd = false,
			CurrentPeriodEnd = endDate,
			Status = "active",
			SubscriptionId = "sub_ok"
		});

		var drift = await BillingHealthWorker.RunPassAsync(builder.Container, NullLogger.Instance, TimeSpan.Zero, CancellationToken.None);

		Assert.Equal(0, drift);
		Assert.Equal(2, builder.ApiClient.Calls.Count);
		Assert.Contains(builder.ReconcileLogger.Entries, e =>
			e.Level == LogLevel.Warning && e.Message.Contains("failed to fetch Stripe subscription"));
	}

	[Fact]
	public async Task RunPassAsync_CancelledLocalSubscriptions_NotChecked()
	{
		var builder = new BillingHealthBuilder();
		var endDate = DateTime.UtcNow.AddDays(-30);

		// Cancelled subscription — should not be reconciled
		await builder.SeedActiveProAsync(userId: 1, stripeSubscriptionId: "sub_cancelled", endDate: endDate, status: FRAMEWORK.SubscriptionStatuses.Cancelled);

		var drift = await BillingHealthWorker.RunPassAsync(builder.Container, NullLogger.Instance, TimeSpan.Zero, CancellationToken.None);

		Assert.Equal(0, drift);
		Assert.Empty(builder.ApiClient.Calls);
	}

	[Fact]
	public async Task RunPassAsync_DriftDetected_StillWritesHeartbeat()
	{
		var builder = new BillingHealthBuilder();
		var endDate = DateTime.UtcNow.AddDays(10);
		await builder.SeedActiveProAsync(userId: 1, stripeSubscriptionId: "sub_drifty", endDate: endDate);
		builder.ApiClient.SetResponse("sub_drifty", new StripeSubscriptionInfo
		{
			CancelAtPeriodEnd = false,
			CurrentPeriodEnd = endDate,
			Status = "canceled",
			SubscriptionId = "sub_drifty"
		});

		await BillingHealthWorker.RunPassAsync(builder.Container, NullLogger.Instance, TimeSpan.Zero, CancellationToken.None);

		var heartbeat = (await builder.HeartbeatAdapter.GetAllAsync()).Single();
		Assert.Equal(BillingHealthWorker.HeartbeatSourceName, heartbeat.HeartbeatSource);
	}

	#endregion
}
