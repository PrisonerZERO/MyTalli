namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class UserAuthenticationAppleMapper : EntityMapper<MODELS.UserAuthenticationApple, ENTITIES.UserAuthenticationApple>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.UserAuthenticationApple source, MODELS.UserAuthenticationApple destination)
	{
		destination.AppleId = source.AppleId;
		destination.DisplayName = source.DisplayName;
		destination.Email = source.Email;
		destination.FirstName = source.FirstName;
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsPrivateRelay = source.IsPrivateRelay;
		destination.IsVisible = source.IsVisible;
		destination.LastName = source.LastName;
	}

	public override ENTITIES.UserAuthenticationApple ToEntity(MODELS.UserAuthenticationApple model) => new()
	{
		AppleId = model.AppleId,
		DisplayName = model.DisplayName,
		Email = model.Email,
		FirstName = model.FirstName,
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsPrivateRelay = model.IsPrivateRelay,
		IsVisible = model.IsVisible,
		LastName = model.LastName,
	};

	public override MODELS.UserAuthenticationApple ToModel(ENTITIES.UserAuthenticationApple entity) => new()
	{
		AppleId = entity.AppleId,
		DisplayName = entity.DisplayName,
		Email = entity.Email,
		FirstName = entity.FirstName,
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsPrivateRelay = entity.IsPrivateRelay,
		IsVisible = entity.IsVisible,
		LastName = entity.LastName,
	};

	#endregion
}
