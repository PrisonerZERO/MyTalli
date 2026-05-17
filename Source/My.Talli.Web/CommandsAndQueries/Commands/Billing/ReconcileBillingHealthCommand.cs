namespace My.Talli.Web.Commands.Billing;

using Domain.Framework;
using Domain.Models;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using Web.Services.Billing;

using ENTITIES = My.Talli.Domain.Entities;

/// <summary>Command</summary>
public class ReconcileBillingHealthCommand
{
	#region <Constants>

	private static readonly TimeSpan DateDriftTolerance = TimeSpan.FromHours(1);

	#endregion

	#region <Variables>

	private readonly ILogger<ReconcileBillingHealthCommand> _logger;
	private readonly IStripeBillingApiClient _stripeClient;
	private readonly RepositoryAdapterAsync<Subscription, ENTITIES.Subscription> _subscriptionAdapter;
	private readonly RepositoryAdapterAsync<SubscriptionStripe, ENTITIES.SubscriptionStripe> _subscriptionStripeAdapter;

	#endregion

	#region <Constructors>

	public ReconcileBillingHealthCommand(
		ILogger<ReconcileBillingHealthCommand> logger,
		IStripeBillingApiClient stripeClient,
		RepositoryAdapterAsync<Subscription, ENTITIES.Subscription> subscriptionAdapter,
		RepositoryAdapterAsync<SubscriptionStripe, ENTITIES.SubscriptionStripe> subscriptionStripeAdapter)
	{
		_logger = logger;
		_stripeClient = stripeClient;
		_subscriptionAdapter = subscriptionAdapter;
		_subscriptionStripeAdapter = subscriptionStripeAdapter;
	}

	#endregion

	#region <Methods>

	public async Task<int> ExecuteAsync(TimeSpan perCallDelay, CancellationToken cancellationToken)
	{
		var locals = (await _subscriptionAdapter.FindAsync(s =>
			s.Status == SubscriptionStatuses.Active || s.Status == SubscriptionStatuses.Cancelling))
			.ToList();

		if (locals.Count == 0)
			return 0;

		var subIds = locals.Select(s => s.Id).ToList();
		var stripeRecords = (await _subscriptionStripeAdapter.FindAsync(r => subIds.Contains(r.Id)))
			.ToDictionary(r => r.Id);

		var driftCount = 0;
		for (var i = 0; i < locals.Count; i++)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var local = locals[i];

			if (!stripeRecords.TryGetValue(local.Id, out var stripeRecord))
			{
				_logger.LogWarning("Billing drift: local Subscription {SubscriptionId} (user {UserId}, status {Status}) has no SubscriptionStripe row.", local.Id, local.UserId, local.Status);
				driftCount++;
				continue;
			}

			StripeSubscriptionInfo? remote;
			try
			{
				remote = await _stripeClient.GetSubscriptionAsync(stripeRecord.StripeSubscriptionId, cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Billing reconciliation: failed to fetch Stripe subscription {StripeSubscriptionId} for local Subscription {SubscriptionId} (user {UserId}).", stripeRecord.StripeSubscriptionId, local.Id, local.UserId);
				continue;
			}

			if (remote is null)
			{
				_logger.LogWarning("Billing drift: Stripe subscription {StripeSubscriptionId} not found, but local Subscription {SubscriptionId} (user {UserId}) is {LocalStatus} with EndDate {EndDate:O}.", stripeRecord.StripeSubscriptionId, local.Id, local.UserId, local.Status, local.EndDate);
				driftCount++;
				continue;
			}

			var driftReason = DetectDrift(local, remote);
			if (driftReason is not null)
			{
				_logger.LogWarning(
					"Billing drift detected for Subscription {SubscriptionId} (user {UserId}, Stripe sub {StripeSubscriptionId}): {Reason}. Local: Status={LocalStatus}, EndDate={LocalEndDate:O}. Stripe: Status={StripeStatus}, CurrentPeriodEnd={StripeEnd:O}, CancelAtPeriodEnd={CancelAtPeriodEnd}.",
					local.Id, local.UserId, stripeRecord.StripeSubscriptionId, driftReason,
					local.Status, local.EndDate,
					remote.Status, remote.CurrentPeriodEnd, remote.CancelAtPeriodEnd);
				driftCount++;
			}

			if (perCallDelay > TimeSpan.Zero && i < locals.Count - 1)
			{
				try { await Task.Delay(perCallDelay, cancellationToken); }
				catch (TaskCanceledException) { return driftCount; }
			}
		}

		return driftCount;
	}

	private static string? DetectDrift(Subscription local, StripeSubscriptionInfo remote)
	{
		var expectedLocalStatus = MapStripeStatusToLocal(remote.Status, remote.CancelAtPeriodEnd);
		if (expectedLocalStatus != local.Status)
			return $"status mismatch (Stripe expects local Status={expectedLocalStatus})";

		var delta = (remote.CurrentPeriodEnd - local.EndDate).Duration();
		if (delta > DateDriftTolerance)
			return $"EndDate mismatch ({delta.TotalMinutes:F0} min apart)";

		return null;
	}

	private static string MapStripeStatusToLocal(string stripeStatus, bool cancelAtPeriodEnd)
	{
		if (cancelAtPeriodEnd && stripeStatus == "active")
			return SubscriptionStatuses.Cancelling;

		return stripeStatus switch
		{
			"active" => SubscriptionStatuses.Active,
			"canceled" => SubscriptionStatuses.Cancelled,
			"past_due" => SubscriptionStatuses.PastDue,
			"unpaid" => SubscriptionStatuses.Unpaid,
			_ => stripeStatus
		};
	}

	#endregion
}
