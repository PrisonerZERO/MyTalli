namespace My.Talli.Domain.Commands.Platforms;

using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class CreateManualShopCommand
{
	#region <Constants>

	private const string ManualPlatformName = "Manual";

	#endregion

	#region <Variables>

	private readonly RepositoryAdapterAsync<PlatformConnection, ENTITIES.PlatformConnection> _platformConnectionAdapter;
	private readonly RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> _shopConnectionAdapter;

	#endregion

	#region <Constructors>

	public CreateManualShopCommand(RepositoryAdapterAsync<PlatformConnection, ENTITIES.PlatformConnection> platformConnectionAdapter, RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> shopConnectionAdapter)
	{
		_platformConnectionAdapter = platformConnectionAdapter;
		_shopConnectionAdapter = shopConnectionAdapter;
	}

	#endregion

	#region <Methods>

	public async Task<ShopConnection> ExecuteAsync(long userId, string shopName)
	{
		var connection = await UpsertPlatformConnectionAsync(userId);
		var shop = await InsertShopAsync(userId, connection.Id, shopName);
		return shop;
	}

	private async Task<PlatformConnection> UpsertPlatformConnectionAsync(long userId)
	{
		var existing = (await _platformConnectionAdapter.FindAsync(p => p.UserId == userId && p.Platform == ManualPlatformName)).FirstOrDefault();

		if (existing is not null)
			return existing;

		return await _platformConnectionAdapter.InsertAsync(new PlatformConnection
		{
			ConnectionStatus = "Active",
			Platform = ManualPlatformName,
			UserId = userId
		});
	}

	private async Task<ShopConnection> InsertShopAsync(long userId, long platformConnectionId, string shopName)
	{
		var syntheticId = $"manual_{Guid.NewGuid():N}";
		var now = DateTime.UtcNow;

		return await _shopConnectionAdapter.InsertAsync(new ShopConnection
		{
			AccessToken = string.Empty,
			ConsecutiveFailures = 0,
			IsActive = true,
			IsEnabled = true,
			NextSyncDateTime = now,
			PlatformAccountId = syntheticId,
			PlatformConnectionId = platformConnectionId,
			PlatformShopId = syntheticId,
			ShopName = shopName,
			Status = "Completed",
			UserId = userId
		});
	}

	#endregion
}
