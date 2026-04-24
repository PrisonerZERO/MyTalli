namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class PayoutStripeMapper : EntityMapper<MODELS.PayoutStripe, ENTITIES.PayoutStripe>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.PayoutStripe source, MODELS.PayoutStripe destination)
	{
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsVisible = source.IsVisible;
		destination.PayoutMethod = source.PayoutMethod;
		destination.StatementDescriptor = source.StatementDescriptor;
		destination.StripePayoutId = source.StripePayoutId;
	}

	public override ENTITIES.PayoutStripe ToEntity(MODELS.PayoutStripe model) => new()
	{
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsVisible = model.IsVisible,
		PayoutMethod = model.PayoutMethod,
		StatementDescriptor = model.StatementDescriptor,
		StripePayoutId = model.StripePayoutId,
	};

	public override MODELS.PayoutStripe ToModel(ENTITIES.PayoutStripe entity) => new()
	{
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsVisible = entity.IsVisible,
		PayoutMethod = entity.PayoutMethod,
		StatementDescriptor = entity.StatementDescriptor,
		StripePayoutId = entity.StripePayoutId,
	};

	#endregion
}
