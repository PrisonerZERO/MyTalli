namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class SubscriptionMapper : EntityMapper<MODELS.Subscription, ENTITIES.Subscription>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.Subscription source, MODELS.Subscription destination)
	{
		destination.CancelledDate = source.CancelledDate;
		destination.EndDate = source.EndDate;
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsVisible = source.IsVisible;
		destination.OrderItemId = source.OrderItemId;
		destination.ProductId = source.ProductId;
		destination.RenewalDate = source.RenewalDate;
		destination.StartDate = source.StartDate;
		destination.Status = source.Status;
		destination.UserId = source.UserId;
	}

	public override ENTITIES.Subscription ToEntity(MODELS.Subscription model) => new()
	{
		CancelledDate = model.CancelledDate,
		EndDate = model.EndDate,
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsVisible = model.IsVisible,
		OrderItemId = model.OrderItemId,
		ProductId = model.ProductId,
		RenewalDate = model.RenewalDate,
		StartDate = model.StartDate,
		Status = model.Status,
		UserId = model.UserId,
	};

	public override MODELS.Subscription ToModel(ENTITIES.Subscription entity) => new()
	{
		CancelledDate = entity.CancelledDate,
		EndDate = entity.EndDate,
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsVisible = entity.IsVisible,
		OrderItemId = entity.OrderItemId,
		ProductId = entity.ProductId,
		RenewalDate = entity.RenewalDate,
		StartDate = entity.StartDate,
		Status = entity.Status,
		UserId = entity.UserId,
	};

	#endregion
}
