namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class RevenueEtsyMapper : EntityMapper<MODELS.RevenueEtsy, ENTITIES.RevenueEtsy>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.RevenueEtsy source, MODELS.RevenueEtsy destination)
	{
		destination.AdjustedFees = source.AdjustedFees;
		destination.AdjustedGross = source.AdjustedGross;
		destination.AdjustedNet = source.AdjustedNet;
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsVisible = source.IsVisible;
		destination.ListingId = source.ListingId;
		destination.ReceiptId = source.ReceiptId;
		destination.ShopCurrency = source.ShopCurrency;
	}

	public override ENTITIES.RevenueEtsy ToEntity(MODELS.RevenueEtsy model) => new()
	{
		AdjustedFees = model.AdjustedFees,
		AdjustedGross = model.AdjustedGross,
		AdjustedNet = model.AdjustedNet,
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsVisible = model.IsVisible,
		ListingId = model.ListingId,
		ReceiptId = model.ReceiptId,
		ShopCurrency = model.ShopCurrency,
	};

	public override MODELS.RevenueEtsy ToModel(ENTITIES.RevenueEtsy entity) => new()
	{
		AdjustedFees = entity.AdjustedFees,
		AdjustedGross = entity.AdjustedGross,
		AdjustedNet = entity.AdjustedNet,
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsVisible = entity.IsVisible,
		ListingId = entity.ListingId,
		ReceiptId = entity.ReceiptId,
		ShopCurrency = entity.ShopCurrency,
	};

	#endregion
}
