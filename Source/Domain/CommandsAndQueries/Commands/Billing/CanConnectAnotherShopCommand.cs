namespace My.Talli.Domain.Commands.Billing;

using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class CanConnectAnotherShopCommand
{
	#region <Variables>

	private readonly IsProSubscriberCommand _isProSubscriber;
	private readonly RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> _shopAdapter;

	#endregion

	#region <Constructors>

	public CanConnectAnotherShopCommand(
		IsProSubscriberCommand isProSubscriber,
		RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> shopAdapter)
	{
		_isProSubscriber = isProSubscriber;
		_shopAdapter = shopAdapter;
	}

	#endregion

	#region <Methods>

	/// <summary>True if the user may connect another shop under their current plan tier. Pro = always true. Free = true only when the user has zero shops across all platforms.</summary>
	public async Task<bool> ExecuteAsync(long userId)
	{
		if (await _isProSubscriber.ExecuteAsync(userId)) return true;

		var totalShopCount = (await _shopAdapter.FindAsync(s => s.UserId == userId)).Count();
		return totalShopCount == 0;
	}

	#endregion
}
