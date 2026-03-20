namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class MilestoneMapper : EntityMapper<MODELS.Milestone, ENTITIES.Milestone>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.Milestone source, MODELS.Milestone destination)
	{
		destination.Description = source.Description;
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsVisible = source.IsVisible;
		destination.MilestoneGroup = source.MilestoneGroup;
		destination.SortOrder = source.SortOrder;
		destination.Status = source.Status;
		destination.Title = source.Title;
	}

	public override ENTITIES.Milestone ToEntity(MODELS.Milestone model) => new()
	{
		Description = model.Description,
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsVisible = model.IsVisible,
		MilestoneGroup = model.MilestoneGroup,
		SortOrder = model.SortOrder,
		Status = model.Status,
		Title = model.Title,
	};

	public override MODELS.Milestone ToModel(ENTITIES.Milestone entity) => new()
	{
		Description = entity.Description,
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsVisible = entity.IsVisible,
		MilestoneGroup = entity.MilestoneGroup,
		SortOrder = entity.SortOrder,
		Status = entity.Status,
		Title = entity.Title,
	};


	#endregion
}
