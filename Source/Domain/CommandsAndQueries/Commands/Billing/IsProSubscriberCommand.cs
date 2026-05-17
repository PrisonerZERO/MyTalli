namespace My.Talli.Domain.Commands.Billing;

using Domain.Framework;
using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class IsProSubscriberCommand
{
	#region <Variables>

	private readonly RepositoryAdapterAsync<Subscription, ENTITIES.Subscription> _subscriptionAdapter;

	#endregion

	#region <Constructors>

	public IsProSubscriberCommand(RepositoryAdapterAsync<Subscription, ENTITIES.Subscription> subscriptionAdapter)
	{
		_subscriptionAdapter = subscriptionAdapter;
	}

	#endregion

	#region <Methods>

	/// <summary>True if the user has an active Pro subscription (ProductId 1 = Pro Monthly or 2 = Pro Yearly, Status Active or Cancelling, and EndDate in the future). Single source of truth for plan-tier checks across the app.</summary>
	public async Task<bool> ExecuteAsync(long userId)
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
