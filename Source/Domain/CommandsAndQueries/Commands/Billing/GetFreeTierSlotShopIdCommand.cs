namespace My.Talli.Domain.Commands.Billing;

using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class GetFreeTierSlotShopIdCommand
{
	#region <Variables>

	private readonly IsProSubscriberCommand _isProSubscriber;
	private readonly RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> _shopAdapter;

	#endregion

	#region <Constructors>

	public GetFreeTierSlotShopIdCommand(
		IsProSubscriberCommand isProSubscriber,
		RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> shopAdapter)
	{
		_isProSubscriber = isProSubscriber;
		_shopAdapter = shopAdapter;
	}

	#endregion

	#region <Methods>

	/// <summary>Returns the user's "free-tier slot" shop id — the oldest ShopConnection (lowest Id, since IDENTITY is monotonic). Returns null for Pro users (no slot to highlight; all shops are unlocked) and for users with no shops. Drives sync gating in ShopSyncWorker and the over-cap badge in PlatformsViewModel.</summary>
	public async Task<long?> ExecuteAsync(long userId)
	{
		if (await _isProSubscriber.ExecuteAsync(userId)) return null;

		var shops = await _shopAdapter.FindAsync(s => s.UserId == userId);
		return shops.OrderBy(s => s.Id).FirstOrDefault()?.Id;
	}

	#endregion
}
