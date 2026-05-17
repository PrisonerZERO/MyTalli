namespace My.Talli.Domain.Commands.Billing;

using Domain.Framework;
using Domain.Models;
using Domain.Repositories;
using System.Collections.Concurrent;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class IsProSubscriberCommand
{
	#region <Variables>

	// Per-circuit cache — this command is scoped (one instance per Blazor circuit), so the cache lives for the
	// user's session. Subsequent ViewModels in the same circuit get the cached result instead of hitting SQL again.
	// Staleness trade-off: if a user's Pro status changes mid-session (Stripe webhook upgrade or natural expiration),
	// they keep the cached status until next full page load. Stripe Checkout success URL is a full reload, so the
	// upgrade path naturally invalidates. Expiration mid-session is a brief UI-only staleness, not a security issue.
	private readonly ConcurrentDictionary<long, Task<bool>> _cache = new();
	private readonly RepositoryAdapterAsync<Subscription, ENTITIES.Subscription> _subscriptionAdapter;

	#endregion

	#region <Constructors>

	public IsProSubscriberCommand(RepositoryAdapterAsync<Subscription, ENTITIES.Subscription> subscriptionAdapter)
	{
		_subscriptionAdapter = subscriptionAdapter;
	}

	#endregion

	#region <Methods>

	/// <summary>True if the user has an active Pro subscription (ProductId 1 = Pro Monthly or 2 = Pro Yearly, Status Active or Cancelling, and EndDate in the future). Single source of truth for plan-tier checks across the app. Result is cached for the lifetime of the Blazor circuit.</summary>
	public Task<bool> ExecuteAsync(long userId)
	{
		return _cache.GetOrAdd(userId, FetchAsync);
	}

	private async Task<bool> FetchAsync(long userId)
	{
		var now = DateTime.UtcNow;

		return (await _subscriptionAdapter.FindAsync(s =>
			s.UserId == userId
			&& (s.ProductId == 1 || s.ProductId == 2)
			&& (s.Status == SubscriptionStatuses.Active || s.Status == SubscriptionStatuses.Cancelling)
			&& s.EndDate >= now)).Any();
	}

	#endregion
}
