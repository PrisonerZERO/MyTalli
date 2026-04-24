namespace My.Talli.Domain.Commands.Platforms;

using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class RenameManualShopCommand
{
	#region <Variables>

	private readonly RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> _shopConnectionAdapter;

	#endregion

	#region <Constructors>

	public RenameManualShopCommand(RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> shopConnectionAdapter)
	{
		_shopConnectionAdapter = shopConnectionAdapter;
	}

	#endregion

	#region <Methods>

	public async Task<ShopConnection?> ExecuteAsync(long userId, long shopConnectionId, string newShopName)
	{
		var name = (newShopName ?? string.Empty).Trim();
		if (name.Length == 0) return null;

		var shop = await _shopConnectionAdapter.GetByIdAsync(shopConnectionId);
		if (shop is null || shop.UserId != userId) return null;

		shop.ShopName = name;
		await _shopConnectionAdapter.UpdateAsync(shop);

		return shop;
	}

	#endregion
}
