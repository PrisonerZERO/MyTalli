namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class GoalMapper : EntityMapper<MODELS.Goal, ENTITIES.Goal>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.Goal source, MODELS.Goal destination)
	{
		destination.CreatedOn = source.CreatedOnDateTime;
		destination.EndDate = source.EndDate;
		destination.GoalTypeId = source.GoalTypeId;
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsVisible = source.IsVisible;
		destination.Platform = source.Platform;
		destination.StartDate = source.StartDate;
		destination.Status = source.Status;
		destination.TargetAmount = source.TargetAmount;
		destination.UserId = source.UserId;
	}

	public override ENTITIES.Goal ToEntity(MODELS.Goal model) => new()
	{
		EndDate = model.EndDate,
		GoalTypeId = model.GoalTypeId,
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsVisible = model.IsVisible,
		Platform = model.Platform,
		StartDate = model.StartDate,
		Status = model.Status,
		TargetAmount = model.TargetAmount,
		UserId = model.UserId,
	};

	public override MODELS.Goal ToModel(ENTITIES.Goal entity) => new()
	{
		CreatedOn = entity.CreatedOnDateTime,
		EndDate = entity.EndDate,
		GoalTypeId = entity.GoalTypeId,
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsVisible = entity.IsVisible,
		Platform = entity.Platform,
		StartDate = entity.StartDate,
		Status = entity.Status,
		TargetAmount = entity.TargetAmount,
		UserId = entity.UserId,
	};

	#endregion
}
