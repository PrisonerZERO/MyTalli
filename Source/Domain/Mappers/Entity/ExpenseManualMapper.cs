namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class ExpenseManualMapper : EntityMapper<MODELS.ExpenseManual, ENTITIES.ExpenseManual>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.ExpenseManual source, MODELS.ExpenseManual destination)
	{
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsVisible = source.IsVisible;
		destination.Notes = source.Notes;
	}

	public override ENTITIES.ExpenseManual ToEntity(MODELS.ExpenseManual model) => new()
	{
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsVisible = model.IsVisible,
		Notes = model.Notes,
	};

	public override MODELS.ExpenseManual ToModel(ENTITIES.ExpenseManual entity) => new()
	{
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsVisible = entity.IsVisible,
		Notes = entity.Notes,
	};

	#endregion
}
