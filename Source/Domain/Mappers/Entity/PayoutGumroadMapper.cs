namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class PayoutGumroadMapper : EntityMapper<MODELS.PayoutGumroad, ENTITIES.PayoutGumroad>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.PayoutGumroad source, MODELS.PayoutGumroad destination)
	{
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsVisible = source.IsVisible;
		destination.PayoutMethod = source.PayoutMethod;
	}

	public override ENTITIES.PayoutGumroad ToEntity(MODELS.PayoutGumroad model) => new()
	{
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsVisible = model.IsVisible,
		PayoutMethod = model.PayoutMethod,
	};

	public override MODELS.PayoutGumroad ToModel(ENTITIES.PayoutGumroad entity) => new()
	{
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsVisible = entity.IsVisible,
		PayoutMethod = entity.PayoutMethod,
	};

	#endregion
}
