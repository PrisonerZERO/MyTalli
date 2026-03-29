namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class RevenueGumroadMapper : EntityMapper<MODELS.RevenueGumroad, ENTITIES.RevenueGumroad>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.RevenueGumroad source, MODELS.RevenueGumroad destination)
	{
		destination.DiscoverFee = source.DiscoverFee;
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsVisible = source.IsVisible;
		destination.LicenseKey = source.LicenseKey;
		destination.SaleId = source.SaleId;
	}

	public override ENTITIES.RevenueGumroad ToEntity(MODELS.RevenueGumroad model) => new()
	{
		DiscoverFee = model.DiscoverFee,
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsVisible = model.IsVisible,
		LicenseKey = model.LicenseKey,
		SaleId = model.SaleId,
	};

	public override MODELS.RevenueGumroad ToModel(ENTITIES.RevenueGumroad entity) => new()
	{
		DiscoverFee = entity.DiscoverFee,
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsVisible = entity.IsVisible,
		LicenseKey = entity.LicenseKey,
		SaleId = entity.SaleId,
	};

	#endregion
}
