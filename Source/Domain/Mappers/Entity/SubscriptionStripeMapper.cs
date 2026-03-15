namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class SubscriptionStripeMapper : EntityMapper<MODELS.SubscriptionStripe, ENTITIES.SubscriptionStripe>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.SubscriptionStripe source, MODELS.SubscriptionStripe destination)
	{
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsVisible = source.IsVisible;
		destination.StripeCustomerId = source.StripeCustomerId;
		destination.StripePriceId = source.StripePriceId;
		destination.StripeSubscriptionId = source.StripeSubscriptionId;
	}

	public override ENTITIES.SubscriptionStripe ToEntity(MODELS.SubscriptionStripe model) => new()
	{
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsVisible = model.IsVisible,
		StripeCustomerId = model.StripeCustomerId,
		StripePriceId = model.StripePriceId,
		StripeSubscriptionId = model.StripeSubscriptionId,
	};

	public override MODELS.SubscriptionStripe ToModel(ENTITIES.SubscriptionStripe entity) => new()
	{
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsVisible = entity.IsVisible,
		StripeCustomerId = entity.StripeCustomerId,
		StripePriceId = entity.StripePriceId,
		StripeSubscriptionId = entity.StripeSubscriptionId,
	};

	#endregion
}
