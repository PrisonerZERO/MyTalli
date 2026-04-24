namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class ExpenseGumroadMapper : EntityMapper<MODELS.ExpenseGumroad, ENTITIES.ExpenseGumroad>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.ExpenseGumroad source, MODELS.ExpenseGumroad destination)
	{
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsVisible = source.IsVisible;
		destination.SubscriptionPlanId = source.SubscriptionPlanId;
	}

	public override ENTITIES.ExpenseGumroad ToEntity(MODELS.ExpenseGumroad model) => new()
	{
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsVisible = model.IsVisible,
		SubscriptionPlanId = model.SubscriptionPlanId,
	};

	public override MODELS.ExpenseGumroad ToModel(ENTITIES.ExpenseGumroad entity) => new()
	{
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsVisible = entity.IsVisible,
		SubscriptionPlanId = entity.SubscriptionPlanId,
	};

	#endregion
}
