namespace My.Talli.Domain.Commands.Billing;

using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class AcknowledgeExpirationCommand
{
	#region <Variables>

	private readonly RepositoryAdapterAsync<Subscription, ENTITIES.Subscription> _subscriptionAdapter;

	#endregion

	#region <Constructors>

	public AcknowledgeExpirationCommand(RepositoryAdapterAsync<Subscription, ENTITIES.Subscription> subscriptionAdapter)
	{
		_subscriptionAdapter = subscriptionAdapter;
	}

	#endregion

	#region <Methods>

	/// <summary>Stamps ExpirationAcknowledgedAt on the subscription if (and only if) the row belongs to userId. Silently no-ops on IDOR mismatch or unknown id.</summary>
	public async Task ExecuteAsync(long subscriptionId, long userId)
	{
		var subscription = await _subscriptionAdapter.GetByIdAsync(subscriptionId);
		if (subscription is null || subscription.UserId != userId) return;

		if (subscription.ExpirationAcknowledgedAt is not null) return;

		subscription.ExpirationAcknowledgedAt = DateTime.UtcNow;
		await _subscriptionAdapter.UpdateAsync(subscription);
	}

	#endregion
}
