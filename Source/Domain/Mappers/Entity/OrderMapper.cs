namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class OrderMapper : EntityMapper<MODELS.Order, ENTITIES.Order>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.Order source, MODELS.Order destination)
	{
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsVisible = source.IsVisible;
		destination.OrderDateTime = source.OrderDateTime;
		destination.TaxCharged = source.TaxCharged;
		destination.UserId = source.UserId;
	}

	public override ENTITIES.Order ToEntity(MODELS.Order model) => new()
	{
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsVisible = model.IsVisible,
		OrderDateTime = model.OrderDateTime,
		TaxCharged = model.TaxCharged,
		UserId = model.UserId,
	};

	public override MODELS.Order ToModel(ENTITIES.Order entity) => new()
	{
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsVisible = entity.IsVisible,
		OrderDateTime = entity.OrderDateTime,
		TaxCharged = entity.TaxCharged,
		UserId = entity.UserId,
	};


	#endregion
}
