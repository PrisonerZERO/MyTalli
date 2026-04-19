namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class PlatformConnectionMapper : EntityMapper<MODELS.PlatformConnection, ENTITIES.PlatformConnection>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.PlatformConnection source, MODELS.PlatformConnection destination)
	{
		destination.ConnectionStatus = source.ConnectionStatus;
		destination.CreatedOn = source.CreatedOnDateTime;
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsVisible = source.IsVisible;
		destination.Platform = source.Platform;
		destination.UserId = source.UserId;
	}

	public override ENTITIES.PlatformConnection ToEntity(MODELS.PlatformConnection model) => new()
	{
		ConnectionStatus = model.ConnectionStatus,
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsVisible = model.IsVisible,
		Platform = model.Platform,
		UserId = model.UserId,
	};

	public override MODELS.PlatformConnection ToModel(ENTITIES.PlatformConnection entity) => new()
	{
		ConnectionStatus = entity.ConnectionStatus,
		CreatedOn = entity.CreatedOnDateTime,
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsVisible = entity.IsVisible,
		Platform = entity.Platform,
		UserId = entity.UserId,
	};

	#endregion
}
