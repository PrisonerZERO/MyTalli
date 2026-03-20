namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class UserAuthenticationGoogleMapper : EntityMapper<MODELS.UserAuthenticationGoogle, ENTITIES.UserAuthenticationGoogle>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.UserAuthenticationGoogle source, MODELS.UserAuthenticationGoogle destination)
	{
		destination.AvatarUrl = source.AvatarUrl;
		destination.DisplayName = source.DisplayName;
		destination.Email = source.Email;
		destination.EmailVerified = source.EmailVerified;
		destination.FirstName = source.FirstName;
		destination.GoogleId = source.GoogleId;
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsVisible = source.IsVisible;
		destination.LastName = source.LastName;
		destination.Locale = source.Locale;
	}

	public override ENTITIES.UserAuthenticationGoogle ToEntity(MODELS.UserAuthenticationGoogle model) => new()
	{
		AvatarUrl = model.AvatarUrl,
		DisplayName = model.DisplayName,
		Email = model.Email,
		EmailVerified = model.EmailVerified,
		FirstName = model.FirstName,
		GoogleId = model.GoogleId,
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsVisible = model.IsVisible,
		LastName = model.LastName,
		Locale = model.Locale,
	};

	public override MODELS.UserAuthenticationGoogle ToModel(ENTITIES.UserAuthenticationGoogle entity) => new()
	{
		AvatarUrl = entity.AvatarUrl,
		DisplayName = entity.DisplayName,
		Email = entity.Email,
		EmailVerified = entity.EmailVerified,
		FirstName = entity.FirstName,
		GoogleId = entity.GoogleId,
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsVisible = entity.IsVisible,
		LastName = entity.LastName,
		Locale = entity.Locale,
	};


	#endregion
}
