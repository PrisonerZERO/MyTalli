namespace My.Talli.Domain.Commands.Billing;

using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class FindExpiredUnacknowledgedSubscriptionCommand
{
	#region <Variables>

	private readonly RepositoryAdapterAsync<Subscription, ENTITIES.Subscription> _subscriptionAdapter;

	#endregion

	#region <Constructors>

	public FindExpiredUnacknowledgedSubscriptionCommand(RepositoryAdapterAsync<Subscription, ENTITIES.Subscription> subscriptionAdapter)
	{
		_subscriptionAdapter = subscriptionAdapter;
	}

	#endregion

	#region <Methods>

	/// <summary>Returns the most-recently-expired Pro subscription for the user that has not yet been acknowledged, or null if none.</summary>
	public async Task<Subscription?> ExecuteAsync(long userId)
	{
		var now = DateTime.UtcNow;

		return (await _subscriptionAdapter.FindAsync(s =>
			s.UserId == userId
			&& (s.ProductId == 1 || s.ProductId == 2)
			&& s.EndDate < now
			&& s.ExpirationAcknowledgedAt == null))
			.OrderByDescending(s => s.EndDate)
			.FirstOrDefault();
	}

	#endregion
}
