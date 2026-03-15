namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class BillingMapper : EntityMapper<MODELS.Billing, ENTITIES.Billing>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.Billing source, MODELS.Billing destination)
	{
		destination.Amount = source.Amount;
		destination.Currency = source.Currency;
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsVisible = source.IsVisible;
		destination.OrderId = source.OrderId;
		destination.Status = source.Status;
		destination.UserId = source.UserId;
	}

	public override ENTITIES.Billing ToEntity(MODELS.Billing model) => new()
	{
		Amount = model.Amount,
		Currency = model.Currency,
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsVisible = model.IsVisible,
		OrderId = model.OrderId,
		Status = model.Status,
		UserId = model.UserId,
	};

	public override MODELS.Billing ToModel(ENTITIES.Billing entity) => new()
	{
		Amount = entity.Amount,
		Currency = entity.Currency,
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsVisible = entity.IsVisible,
		OrderId = entity.OrderId,
		Status = entity.Status,
		UserId = entity.UserId,
	};

	#endregion
}
