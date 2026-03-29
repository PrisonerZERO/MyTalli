namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class ExpenseMapper : EntityMapper<MODELS.Expense, ENTITIES.Expense>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.Expense source, MODELS.Expense destination)
	{
		destination.Amount = source.Amount;
		destination.Category = source.Category;
		destination.CreatedOn = source.CreatedOnDateTime;
		destination.Currency = source.Currency;
		destination.Description = source.Description;
		destination.ExpenseDate = source.ExpenseDate;
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsVisible = source.IsVisible;
		destination.Platform = source.Platform;
		destination.PlatformTransactionId = source.PlatformTransactionId;
		destination.UserId = source.UserId;
	}

	public override ENTITIES.Expense ToEntity(MODELS.Expense model) => new()
	{
		Amount = model.Amount,
		Category = model.Category,
		Currency = model.Currency,
		Description = model.Description,
		ExpenseDate = model.ExpenseDate,
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsVisible = model.IsVisible,
		Platform = model.Platform,
		PlatformTransactionId = model.PlatformTransactionId,
		UserId = model.UserId,
	};

	public override MODELS.Expense ToModel(ENTITIES.Expense entity) => new()
	{
		Amount = entity.Amount,
		Category = entity.Category,
		CreatedOn = entity.CreatedOnDateTime,
		Currency = entity.Currency,
		Description = entity.Description,
		ExpenseDate = entity.ExpenseDate,
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsVisible = entity.IsVisible,
		Platform = entity.Platform,
		PlatformTransactionId = entity.PlatformTransactionId,
		UserId = entity.UserId,
	};

	#endregion
}
