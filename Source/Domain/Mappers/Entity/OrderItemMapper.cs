namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class OrderItemMapper : EntityMapperBase<MODELS.OrderItem, ENTITIES.OrderItem>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.OrderItem source, MODELS.OrderItem destination)
	{
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsVisible = source.IsVisible;
		destination.OrderId = source.OrderId;
		destination.ProductId = source.ProductId;
		destination.ProductPriceCharged = source.ProductPriceCharged;
		destination.ProductQuantity = source.ProductQuantity;
	}

	public override ENTITIES.OrderItem ToEntity(MODELS.OrderItem model) => new()
	{
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsVisible = model.IsVisible,
		OrderId = model.OrderId,
		ProductId = model.ProductId,
		ProductPriceCharged = model.ProductPriceCharged,
		ProductQuantity = model.ProductQuantity,
	};

	public override MODELS.OrderItem ToModel(ENTITIES.OrderItem entity) => new()
	{
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsVisible = entity.IsVisible,
		OrderId = entity.OrderId,
		ProductId = entity.ProductId,
		ProductPriceCharged = entity.ProductPriceCharged,
		ProductQuantity = entity.ProductQuantity,
	};

	#endregion
}
