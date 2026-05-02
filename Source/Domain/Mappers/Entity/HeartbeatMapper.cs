namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class HeartbeatMapper : EntityMapper<MODELS.Heartbeat, ENTITIES.Heartbeat>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.Heartbeat source, MODELS.Heartbeat destination)
	{
		destination.ExpectedIntervalSeconds = source.ExpectedIntervalSeconds;
		destination.HeartbeatSource = source.HeartbeatSource;
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsVisible = source.IsVisible;
		destination.LastTickAt = source.LastTickAt;
		destination.Metadata = source.Metadata;
	}

	public override ENTITIES.Heartbeat ToEntity(MODELS.Heartbeat model) => new()
	{
		ExpectedIntervalSeconds = model.ExpectedIntervalSeconds,
		HeartbeatSource = model.HeartbeatSource,
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsVisible = model.IsVisible,
		LastTickAt = model.LastTickAt,
		Metadata = model.Metadata,
	};

	public override MODELS.Heartbeat ToModel(ENTITIES.Heartbeat entity) => new()
	{
		ExpectedIntervalSeconds = entity.ExpectedIntervalSeconds,
		HeartbeatSource = entity.HeartbeatSource,
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsVisible = entity.IsVisible,
		LastTickAt = entity.LastTickAt,
		Metadata = entity.Metadata,
	};


	#endregion
}
