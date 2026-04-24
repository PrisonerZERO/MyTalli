namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class PayoutManualMapper : EntityMapper<MODELS.PayoutManual, ENTITIES.PayoutManual>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.PayoutManual source, MODELS.PayoutManual destination)
	{
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsVisible = source.IsVisible;
		destination.Notes = source.Notes;
	}

	public override ENTITIES.PayoutManual ToEntity(MODELS.PayoutManual model) => new()
	{
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsVisible = model.IsVisible,
		Notes = model.Notes,
	};

	public override MODELS.PayoutManual ToModel(ENTITIES.PayoutManual entity) => new()
	{
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsVisible = entity.IsVisible,
		Notes = entity.Notes,
	};

	#endregion
}
