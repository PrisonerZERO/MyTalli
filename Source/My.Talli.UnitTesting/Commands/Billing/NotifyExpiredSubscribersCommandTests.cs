namespace My.Talli.UnitTesting.Commands.Billing;

using Microsoft.Extensions.Logging;
using My.Talli.UnitTesting.Infrastructure.Builders;

/// <summary>Tests</summary>
public class NotifyExpiredSubscribersCommandTests
{
	#region <Methods>

	[Fact]
	public async Task ExecuteAsync_NoExpiredSubs_SendsNothing()
	{
		var builder = new BillingHealthBuilder();

		var sent = await builder.NotifyExpiredSubscribers.ExecuteAsync(CancellationToken.None);

		Assert.Equal(0, sent);
		Assert.Empty(builder.EmailService.Sent);
	}

	[Fact]
	public async Task ExecuteAsync_ExpiredProWithGoogleUser_SendsEmailAndMarksSent()
	{
		var builder = new BillingHealthBuilder();
		var userId = await builder.SeedGoogleUserAsync("Robert", "robert@example.com");
		var sub = await builder.SeedActiveProAsync(userId, "sub_test", DateTime.UtcNow.AddDays(-2));

		var sent = await builder.NotifyExpiredSubscribers.ExecuteAsync(CancellationToken.None);

		Assert.Equal(1, sent);
		Assert.Single(builder.EmailService.Sent);
		Assert.Contains("robert@example.com", builder.EmailService.Sent[0].To);
		Assert.Contains("Pro Monthly", builder.EmailService.Sent[0].Subject);

		var reloaded = (await builder.SubscriptionAdapter.GetByIdAsync(sub.Id))!;
		Assert.NotNull(reloaded.ExpirationEmailSentAt);
	}

	[Fact]
	public async Task ExecuteAsync_AlreadyEmailed_DoesNotResend()
	{
		var builder = new BillingHealthBuilder();
		var userId = await builder.SeedGoogleUserAsync("Robert", "robert@example.com");
		var sub = await builder.SeedActiveProAsync(userId, "sub_test", DateTime.UtcNow.AddDays(-2));

		// Mark as already-sent
		sub.ExpirationEmailSentAt = DateTime.UtcNow.AddHours(-12);
		await builder.SubscriptionAdapter.UpdateAsync(sub);

		var sent = await builder.NotifyExpiredSubscribers.ExecuteAsync(CancellationToken.None);

		Assert.Equal(0, sent);
		Assert.Empty(builder.EmailService.Sent);
	}

	[Fact]
	public async Task ExecuteAsync_NotYetExpired_DoesNotSend()
	{
		var builder = new BillingHealthBuilder();
		var userId = await builder.SeedGoogleUserAsync("Robert", "robert@example.com");
		await builder.SeedActiveProAsync(userId, "sub_test", DateTime.UtcNow.AddDays(15));

		var sent = await builder.NotifyExpiredSubscribers.ExecuteAsync(CancellationToken.None);

		Assert.Equal(0, sent);
		Assert.Empty(builder.EmailService.Sent);
	}

	[Fact]
	public async Task ExecuteAsync_MissingUserRow_LogsWarningAndContinues()
	{
		var builder = new BillingHealthBuilder();
		// Seed a subscription without an associated User row (orphan)
		var sub = await builder.SeedActiveProAsync(userId: 999, "sub_orphan", DateTime.UtcNow.AddDays(-2));

		var sent = await builder.NotifyExpiredSubscribers.ExecuteAsync(CancellationToken.None);

		Assert.Equal(0, sent);
		Assert.Empty(builder.EmailService.Sent);
		Assert.Contains(builder.NotifyLogger.Entries, e =>
			e.Level == LogLevel.Warning && e.Message.Contains("no User row found"));

		var reloaded = (await builder.SubscriptionAdapter.GetByIdAsync(sub.Id))!;
		Assert.Null(reloaded.ExpirationEmailSentAt);
	}

	[Fact]
	public async Task ExecuteAsync_MultipleExpired_SendsToEachAndMarksAll()
	{
		var builder = new BillingHealthBuilder();
		var u1 = await builder.SeedGoogleUserAsync("Alice", "alice@example.com");
		var u2 = await builder.SeedGoogleUserAsync("Bob", "bob@example.com");
		await builder.SeedActiveProAsync(u1, "sub_alice", DateTime.UtcNow.AddDays(-1));
		await builder.SeedActiveProAsync(u2, "sub_bob", DateTime.UtcNow.AddDays(-1));

		var sent = await builder.NotifyExpiredSubscribers.ExecuteAsync(CancellationToken.None);

		Assert.Equal(2, sent);
		Assert.Equal(2, builder.EmailService.Sent.Count);
	}

	#endregion
}
