namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class RevenueStripeMapper : EntityMapper<MODELS.RevenueStripe, ENTITIES.RevenueStripe>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.RevenueStripe source, MODELS.RevenueStripe destination)
	{
		destination.BalanceTransactionId = source.BalanceTransactionId;
		destination.ExchangeRate = source.ExchangeRate;
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsVisible = source.IsVisible;
		destination.PaymentMethod = source.PaymentMethod;
		destination.RiskScore = source.RiskScore;
	}

	public override ENTITIES.RevenueStripe ToEntity(MODELS.RevenueStripe model) => new()
	{
		BalanceTransactionId = model.BalanceTransactionId,
		ExchangeRate = model.ExchangeRate,
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsVisible = model.IsVisible,
		PaymentMethod = model.PaymentMethod,
		RiskScore = model.RiskScore,
	};

	public override MODELS.RevenueStripe ToModel(ENTITIES.RevenueStripe entity) => new()
	{
		BalanceTransactionId = entity.BalanceTransactionId,
		ExchangeRate = entity.ExchangeRate,
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsVisible = entity.IsVisible,
		PaymentMethod = entity.PaymentMethod,
		RiskScore = entity.RiskScore,
	};

	#endregion
}
