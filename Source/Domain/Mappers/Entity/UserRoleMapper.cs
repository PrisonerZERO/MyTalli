namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class UserRoleMapper : EntityMapper<MODELS.UserRole, ENTITIES.UserRole>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.UserRole source, MODELS.UserRole destination)
	{
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsVisible = source.IsVisible;
		destination.Role = source.Role;
		destination.UserId = source.UserId;
	}

	public override ENTITIES.UserRole ToEntity(MODELS.UserRole model) => new()
	{
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsVisible = model.IsVisible,
		Role = model.Role,
		UserId = model.UserId,
	};

	public override MODELS.UserRole ToModel(ENTITIES.UserRole entity) => new()
	{
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsVisible = entity.IsVisible,
		Role = entity.Role,
		UserId = entity.UserId,
	};

	#endregion
}
