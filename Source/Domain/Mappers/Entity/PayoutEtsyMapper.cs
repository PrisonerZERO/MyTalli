namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class PayoutEtsyMapper : EntityMapper<MODELS.PayoutEtsy, ENTITIES.PayoutEtsy>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.PayoutEtsy source, MODELS.PayoutEtsy destination)
	{
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsVisible = source.IsVisible;
		destination.LedgerEntryId = source.LedgerEntryId;
		destination.ShopCurrency = source.ShopCurrency;
	}

	public override ENTITIES.PayoutEtsy ToEntity(MODELS.PayoutEtsy model) => new()
	{
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsVisible = model.IsVisible,
		LedgerEntryId = model.LedgerEntryId,
		ShopCurrency = model.ShopCurrency,
	};

	public override MODELS.PayoutEtsy ToModel(ENTITIES.PayoutEtsy entity) => new()
	{
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsVisible = entity.IsVisible,
		LedgerEntryId = entity.LedgerEntryId,
		ShopCurrency = entity.ShopCurrency,
	};

	#endregion
}
