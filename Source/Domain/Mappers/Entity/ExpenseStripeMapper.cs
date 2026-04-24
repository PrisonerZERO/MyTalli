namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class ExpenseStripeMapper : EntityMapper<MODELS.ExpenseStripe, ENTITIES.ExpenseStripe>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.ExpenseStripe source, MODELS.ExpenseStripe destination)
	{
		destination.BalanceTransactionId = source.BalanceTransactionId;
		destination.FeeType = source.FeeType;
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsVisible = source.IsVisible;
	}

	public override ENTITIES.ExpenseStripe ToEntity(MODELS.ExpenseStripe model) => new()
	{
		BalanceTransactionId = model.BalanceTransactionId,
		FeeType = model.FeeType,
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsVisible = model.IsVisible,
	};

	public override MODELS.ExpenseStripe ToModel(ENTITIES.ExpenseStripe entity) => new()
	{
		BalanceTransactionId = entity.BalanceTransactionId,
		FeeType = entity.FeeType,
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsVisible = entity.IsVisible,
	};

	#endregion
}
