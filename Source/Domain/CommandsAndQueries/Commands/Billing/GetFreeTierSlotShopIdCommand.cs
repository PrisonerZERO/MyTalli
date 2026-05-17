namespace My.Talli.Domain.Commands.Billing;

using Domain.Framework;
using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class GetFreeTierSlotShopIdCommand
{
	#region <Variables>

	private readonly RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> _shopAdapter;
	private readonly RepositoryAdapterAsync<Subscription, ENTITIES.Subscription> _subscriptionAdapter;

	#endregion

	#region <Constructors>

	public GetFreeTierSlotShopIdCommand(
		RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> shopAdapter,
		RepositoryAdapterAsync<Subscription, ENTITIES.Subscription> subscriptionAdapter)
	{
		_shopAdapter = shopAdapter;
		_subscriptionAdapter = subscriptionAdapter;
	}

	#endregion

	#region <Methods>

	/// <summary>Returns the user's "free-tier slot" shop id — the oldest ShopConnection (lowest Id, since IDENTITY is monotonic). Returns null for Pro users (no slot to highlight; all shops are unlocked) and for users with no shops. Drives sync gating in ShopSyncWorker and the over-cap badge in PlatformsViewModel.</summary>
	public async Task<long?> ExecuteAsync(long userId)
	{
		var now = DateTime.UtcNow;

		var isPro = (await _subscriptionAdapter.FindAsync(s =>
			s.UserId == userId
			&& (s.ProductId == 1 || s.ProductId == 2)
			&& (s.Status == SubscriptionStatuses.Active || s.Status == SubscriptionStatuses.Cancelling)
			&& s.EndDate >= now)).Any();

		if (isPro) return null;

		var shops = await _shopAdapter.FindAsync(s => s.UserId == userId);
		return shops.OrderBy(s => s.Id).FirstOrDefault()?.Id;
	}

	#endregion
}
