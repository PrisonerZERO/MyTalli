namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class ShopConnectionMapper : EntityMapper<MODELS.ShopConnection, ENTITIES.ShopConnection>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.ShopConnection source, MODELS.ShopConnection destination)
	{
		destination.ConsecutiveFailures = source.ConsecutiveFailures;
		destination.CreatedOn = source.CreatedOnDateTime;
		destination.Id = source.Id;
		destination.IsActive = source.IsActive;
		destination.IsDeleted = source.IsDeleted;
		destination.IsEnabled = source.IsEnabled;
		destination.IsVisible = source.IsVisible;
		destination.LastErrorMessage = source.LastErrorMessage;
		destination.LastSyncDateTime = source.LastSyncDateTime;
		destination.NextSyncDateTime = source.NextSyncDateTime;
		destination.PlatformConnectionId = source.PlatformConnectionId;
		destination.PlatformShopId = source.PlatformShopId;
		destination.ShopName = source.ShopName;
		destination.Status = source.Status;
		destination.UserId = source.UserId;
	}

	public override ENTITIES.ShopConnection ToEntity(MODELS.ShopConnection model) => new()
	{
		ConsecutiveFailures = model.ConsecutiveFailures,
		Id = model.Id,
		IsActive = model.IsActive,
		IsDeleted = model.IsDeleted,
		IsEnabled = model.IsEnabled,
		IsVisible = model.IsVisible,
		LastErrorMessage = model.LastErrorMessage,
		LastSyncDateTime = model.LastSyncDateTime,
		NextSyncDateTime = model.NextSyncDateTime,
		PlatformConnectionId = model.PlatformConnectionId,
		PlatformShopId = model.PlatformShopId,
		ShopName = model.ShopName,
		Status = model.Status,
		UserId = model.UserId,
	};

	public override MODELS.ShopConnection ToModel(ENTITIES.ShopConnection entity) => new()
	{
		ConsecutiveFailures = entity.ConsecutiveFailures,
		CreatedOn = entity.CreatedOnDateTime,
		Id = entity.Id,
		IsActive = entity.IsActive,
		IsDeleted = entity.IsDeleted,
		IsEnabled = entity.IsEnabled,
		IsVisible = entity.IsVisible,
		LastErrorMessage = entity.LastErrorMessage,
		LastSyncDateTime = entity.LastSyncDateTime,
		NextSyncDateTime = entity.NextSyncDateTime,
		PlatformConnectionId = entity.PlatformConnectionId,
		PlatformShopId = entity.PlatformShopId,
		ShopName = entity.ShopName,
		Status = entity.Status,
		UserId = entity.UserId,
	};

	#endregion
}
