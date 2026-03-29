namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class GoalTypeMapper : EntityMapper<MODELS.GoalType, ENTITIES.GoalType>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.GoalType source, MODELS.GoalType destination)
	{
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsVisible = source.IsVisible;
		destination.Name = source.Name;
	}

	public override ENTITIES.GoalType ToEntity(MODELS.GoalType model) => new()
	{
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsVisible = model.IsVisible,
		Name = model.Name,
	};

	public override MODELS.GoalType ToModel(ENTITIES.GoalType entity) => new()
	{
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsVisible = entity.IsVisible,
		Name = entity.Name,
	};

	#endregion
}
