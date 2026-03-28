namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class SuggestionMapper : EntityMapper<MODELS.Suggestion, ENTITIES.Suggestion>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.Suggestion source, MODELS.Suggestion destination)
	{
		destination.AdminNote = source.AdminNote;
		destination.Category = source.Category;
		destination.CreatedOn = source.CreatedOnDateTime;
		destination.Description = source.Description;
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsVisible = source.IsVisible;
		destination.Status = source.Status;
		destination.Title = source.Title;
		destination.UserId = source.UserId;
	}

	public override ENTITIES.Suggestion ToEntity(MODELS.Suggestion model) => new()
	{
		AdminNote = model.AdminNote,
		Category = model.Category,
		Description = model.Description,
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsVisible = model.IsVisible,
		Status = model.Status,
		Title = model.Title,
		UserId = model.UserId,
	};

	public override MODELS.Suggestion ToModel(ENTITIES.Suggestion entity) => new()
	{
		AdminNote = entity.AdminNote,
		Category = entity.Category,
		CreatedOn = entity.CreatedOnDateTime,
		Description = entity.Description,
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsVisible = entity.IsVisible,
		Status = entity.Status,
		Title = entity.Title,
		UserId = entity.UserId,
	};


	#endregion
}
