namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class SyncQueueMapper : EntityMapper<MODELS.SyncQueue, ENTITIES.SyncQueue>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.SyncQueue source, MODELS.SyncQueue destination)
	{
		destination.ConsecutiveFailures = source.ConsecutiveFailures;
		destination.CreatedOn = source.CreatedOnDateTime;
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsEnabled = source.IsEnabled;
		destination.IsVisible = source.IsVisible;
		destination.LastErrorMessage = source.LastErrorMessage;
		destination.LastSyncDateTime = source.LastSyncDateTime;
		destination.NextSyncDateTime = source.NextSyncDateTime;
		destination.Platform = source.Platform;
		destination.Status = source.Status;
		destination.UserId = source.UserId;
	}

	public override ENTITIES.SyncQueue ToEntity(MODELS.SyncQueue model) => new()
	{
		ConsecutiveFailures = model.ConsecutiveFailures,
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsEnabled = model.IsEnabled,
		IsVisible = model.IsVisible,
		LastErrorMessage = model.LastErrorMessage,
		LastSyncDateTime = model.LastSyncDateTime,
		NextSyncDateTime = model.NextSyncDateTime,
		Platform = model.Platform,
		Status = model.Status,
		UserId = model.UserId,
	};

	public override MODELS.SyncQueue ToModel(ENTITIES.SyncQueue entity) => new()
	{
		ConsecutiveFailures = entity.ConsecutiveFailures,
		CreatedOn = entity.CreatedOnDateTime,
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsEnabled = entity.IsEnabled,
		IsVisible = entity.IsVisible,
		LastErrorMessage = entity.LastErrorMessage,
		LastSyncDateTime = entity.LastSyncDateTime,
		NextSyncDateTime = entity.NextSyncDateTime,
		Platform = entity.Platform,
		Status = entity.Status,
		UserId = entity.UserId,
	};

	#endregion
}
