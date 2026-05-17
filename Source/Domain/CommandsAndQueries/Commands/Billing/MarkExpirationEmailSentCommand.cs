namespace My.Talli.Domain.Commands.Billing;

using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class MarkExpirationEmailSentCommand
{
	#region <Variables>

	private readonly RepositoryAdapterAsync<Subscription, ENTITIES.Subscription> _subscriptionAdapter;

	#endregion

	#region <Constructors>

	public MarkExpirationEmailSentCommand(RepositoryAdapterAsync<Subscription, ENTITIES.Subscription> subscriptionAdapter)
	{
		_subscriptionAdapter = subscriptionAdapter;
	}

	#endregion

	#region <Methods>

	public async Task ExecuteAsync(long subscriptionId)
	{
		var subscription = await _subscriptionAdapter.GetByIdAsync(subscriptionId);
		if (subscription is null) return;

		subscription.ExpirationEmailSentAt = DateTime.UtcNow;
		await _subscriptionAdapter.UpdateAsync(subscription);
	}

	#endregion
}
