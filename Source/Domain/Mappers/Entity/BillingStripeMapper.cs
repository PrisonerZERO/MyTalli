namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class BillingStripeMapper : EntityMapper<MODELS.BillingStripe, ENTITIES.BillingStripe>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.BillingStripe source, MODELS.BillingStripe destination)
	{
		destination.CardBrand = source.CardBrand;
		destination.CardLastFour = source.CardLastFour;
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsVisible = source.IsVisible;
		destination.PaymentMethod = source.PaymentMethod;
		destination.StripePaymentIntentId = source.StripePaymentIntentId;
	}

	public override ENTITIES.BillingStripe ToEntity(MODELS.BillingStripe model) => new()
	{
		CardBrand = model.CardBrand,
		CardLastFour = model.CardLastFour,
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsVisible = model.IsVisible,
		PaymentMethod = model.PaymentMethod,
		StripePaymentIntentId = model.StripePaymentIntentId,
	};

	public override MODELS.BillingStripe ToModel(ENTITIES.BillingStripe entity) => new()
	{
		CardBrand = entity.CardBrand,
		CardLastFour = entity.CardLastFour,
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsVisible = entity.IsVisible,
		PaymentMethod = entity.PaymentMethod,
		StripePaymentIntentId = entity.StripePaymentIntentId,
	};

	#endregion
}
