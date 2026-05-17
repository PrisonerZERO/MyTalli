namespace My.Talli.Domain.Commands.Billing;

using Domain.Framework;
using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class CanConnectAnotherShopCommand
{
	#region <Variables>

	private readonly RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> _shopAdapter;
	private readonly RepositoryAdapterAsync<Subscription, ENTITIES.Subscription> _subscriptionAdapter;

	#endregion

	#region <Constructors>

	public CanConnectAnotherShopCommand(
		RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> shopAdapter,
		RepositoryAdapterAsync<Subscription, ENTITIES.Subscription> subscriptionAdapter)
	{
		_shopAdapter = shopAdapter;
		_subscriptionAdapter = subscriptionAdapter;
	}

	#endregion

	#region <Methods>

	/// <summary>True if the user may connect another shop under their current plan tier. Pro (Active or Cancelling with EndDate in the future) = always true. Free = true only when the user has zero shops across all platforms.</summary>
	public async Task<bool> ExecuteAsync(long userId)
	{
		var now = DateTime.UtcNow;

		var isPro = (await _subscriptionAdapter.FindAsync(s =>
			s.UserId == userId
			&& (s.ProductId == 1 || s.ProductId == 2)
			&& (s.Status == SubscriptionStatuses.Active || s.Status == SubscriptionStatuses.Cancelling)
			&& s.EndDate >= now)).Any();

		if (isPro) return true;

		var totalShopCount = (await _shopAdapter.FindAsync(s => s.UserId == userId)).Count();
		return totalShopCount == 0;
	}

	#endregion
}
