namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class SuggestionVoteMapper : EntityMapper<MODELS.SuggestionVote, ENTITIES.SuggestionVote>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.SuggestionVote source, MODELS.SuggestionVote destination)
	{
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsVisible = source.IsVisible;
		destination.SuggestionId = source.SuggestionId;
		destination.UserId = source.UserId;
	}

	public override ENTITIES.SuggestionVote ToEntity(MODELS.SuggestionVote model) => new()
	{
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsVisible = model.IsVisible,
		SuggestionId = model.SuggestionId,
		UserId = model.UserId,
	};

	public override MODELS.SuggestionVote ToModel(ENTITIES.SuggestionVote entity) => new()
	{
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsVisible = entity.IsVisible,
		SuggestionId = entity.SuggestionId,
		UserId = entity.UserId,
	};


	#endregion
}
