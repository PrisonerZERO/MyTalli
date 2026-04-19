namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class ShopConnectionMapper : EntityMapper<MODELS.ShopConnection, ENTITIES.ShopConnection>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.ShopConnection source, MODELS.ShopConnection destination)
	{
		destination.AccessToken = source.AccessToken;
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
		destination.PlatformAccountId = source.PlatformAccountId;
		destination.PlatformConnectionId = source.PlatformConnectionId;
		destination.PlatformShopId = source.PlatformShopId;
		destination.RefreshToken = source.RefreshToken;
		destination.RefreshTokenExpiryDateTime = source.RefreshTokenExpiryDateTime;
		destination.ShopName = source.ShopName;
		destination.Status = source.Status;
		destination.TokenExpiryDateTime = source.TokenExpiryDateTime;
		destination.UserId = source.UserId;
	}

	public override ENTITIES.ShopConnection ToEntity(MODELS.ShopConnection model) => new()
	{
		AccessToken = model.AccessToken,
		ConsecutiveFailures = model.ConsecutiveFailures,
		Id = model.Id,
		IsActive = model.IsActive,
		IsDeleted = model.IsDeleted,
		IsEnabled = model.IsEnabled,
		IsVisible = model.IsVisible,
		LastErrorMessage = model.LastErrorMessage,
		LastSyncDateTime = model.LastSyncDateTime,
		NextSyncDateTime = model.NextSyncDateTime,
		PlatformAccountId = model.PlatformAccountId,
		PlatformConnectionId = model.PlatformConnectionId,
		PlatformShopId = model.PlatformShopId,
		RefreshToken = model.RefreshToken,
		RefreshTokenExpiryDateTime = model.RefreshTokenExpiryDateTime,
		ShopName = model.ShopName,
		Status = model.Status,
		TokenExpiryDateTime = model.TokenExpiryDateTime,
		UserId = model.UserId,
	};

	public override MODELS.ShopConnection ToModel(ENTITIES.ShopConnection entity) => new()
	{
		AccessToken = entity.AccessToken,
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
		PlatformAccountId = entity.PlatformAccountId,
		PlatformConnectionId = entity.PlatformConnectionId,
		PlatformShopId = entity.PlatformShopId,
		RefreshToken = entity.RefreshToken,
		RefreshTokenExpiryDateTime = entity.RefreshTokenExpiryDateTime,
		ShopName = entity.ShopName,
		Status = entity.Status,
		TokenExpiryDateTime = entity.TokenExpiryDateTime,
		UserId = entity.UserId,
	};

	#endregion
}
