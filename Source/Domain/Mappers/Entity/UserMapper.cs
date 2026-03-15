namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class UserMapper : EntityMapperBase<MODELS.User, ENTITIES.User>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.User source, MODELS.User destination)
	{
		destination.DisplayName = source.DisplayName;
		destination.FirstName = source.FirstName;
		destination.Id = source.Id;
		destination.InitialProvider = source.InitialProvider;
		destination.IsDeleted = source.IsDeleted;
		destination.IsVisible = source.IsVisible;
		destination.LastLoginAt = source.LastLoginAt;
		destination.LastName = source.LastName;
		destination.PreferredProvider = source.PreferredProvider;
		destination.UserPreferences = source.UserPreferences;
	}

	public override ENTITIES.User ToEntity(MODELS.User model) => new()
	{
		DisplayName = model.DisplayName,
		FirstName = model.FirstName,
		Id = model.Id,
		InitialProvider = model.InitialProvider,
		IsDeleted = model.IsDeleted,
		IsVisible = model.IsVisible,
		LastLoginAt = model.LastLoginAt,
		LastName = model.LastName,
		PreferredProvider = model.PreferredProvider,
		UserPreferences = model.UserPreferences,
	};

	public override MODELS.User ToModel(ENTITIES.User entity) => new()
	{
		DisplayName = entity.DisplayName,
		FirstName = entity.FirstName,
		Id = entity.Id,
		InitialProvider = entity.InitialProvider,
		IsDeleted = entity.IsDeleted,
		IsVisible = entity.IsVisible,
		LastLoginAt = entity.LastLoginAt,
		LastName = entity.LastName,
		PreferredProvider = entity.PreferredProvider,
		UserPreferences = entity.UserPreferences,
	};

	#endregion
}
