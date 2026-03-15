namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class UserAuthenticationMicrosoftMapper : EntityMapperBase<MODELS.UserAuthenticationMicrosoft, ENTITIES.UserAuthenticationMicrosoft>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.UserAuthenticationMicrosoft source, MODELS.UserAuthenticationMicrosoft destination)
	{
		destination.DisplayName = source.DisplayName;
		destination.Email = source.Email;
		destination.FirstName = source.FirstName;
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsVisible = source.IsVisible;
		destination.LastName = source.LastName;
		destination.MicrosoftId = source.MicrosoftId;
	}

	public override ENTITIES.UserAuthenticationMicrosoft ToEntity(MODELS.UserAuthenticationMicrosoft model) => new()
	{
		DisplayName = model.DisplayName,
		Email = model.Email,
		FirstName = model.FirstName,
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsVisible = model.IsVisible,
		LastName = model.LastName,
		MicrosoftId = model.MicrosoftId,
	};

	public override MODELS.UserAuthenticationMicrosoft ToModel(ENTITIES.UserAuthenticationMicrosoft entity) => new()
	{
		DisplayName = entity.DisplayName,
		Email = entity.Email,
		FirstName = entity.FirstName,
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsVisible = entity.IsVisible,
		LastName = entity.LastName,
		MicrosoftId = entity.MicrosoftId,
	};

	#endregion
}
