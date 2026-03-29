namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class PayoutMapper : EntityMapper<MODELS.Payout, ENTITIES.Payout>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.Payout source, MODELS.Payout destination)
	{
		destination.Amount = source.Amount;
		destination.CreatedOn = source.CreatedOnDateTime;
		destination.Currency = source.Currency;
		destination.ExpectedArrivalDate = source.ExpectedArrivalDate;
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsVisible = source.IsVisible;
		destination.PayoutDate = source.PayoutDate;
		destination.Platform = source.Platform;
		destination.PlatformPayoutId = source.PlatformPayoutId;
		destination.Status = source.Status;
		destination.UserId = source.UserId;
	}

	public override ENTITIES.Payout ToEntity(MODELS.Payout model) => new()
	{
		Amount = model.Amount,
		Currency = model.Currency,
		ExpectedArrivalDate = model.ExpectedArrivalDate,
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsVisible = model.IsVisible,
		PayoutDate = model.PayoutDate,
		Platform = model.Platform,
		PlatformPayoutId = model.PlatformPayoutId,
		Status = model.Status,
		UserId = model.UserId,
	};

	public override MODELS.Payout ToModel(ENTITIES.Payout entity) => new()
	{
		Amount = entity.Amount,
		CreatedOn = entity.CreatedOnDateTime,
		Currency = entity.Currency,
		ExpectedArrivalDate = entity.ExpectedArrivalDate,
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsVisible = entity.IsVisible,
		PayoutDate = entity.PayoutDate,
		Platform = entity.Platform,
		PlatformPayoutId = entity.PlatformPayoutId,
		Status = entity.Status,
		UserId = entity.UserId,
	};

	#endregion
}
