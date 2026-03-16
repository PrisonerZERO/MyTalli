namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class ProductVendorMapper : EntityMapper<MODELS.ProductVendor, ENTITIES.ProductVendor>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.ProductVendor source, MODELS.ProductVendor destination)
	{
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsVisible = source.IsVisible;
		destination.VendorName = source.VendorName;
	}

	public override ENTITIES.ProductVendor ToEntity(MODELS.ProductVendor model) => new()
	{
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsVisible = model.IsVisible,
		VendorName = model.VendorName,
	};

	public override MODELS.ProductVendor ToModel(ENTITIES.ProductVendor entity) => new()
	{
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsVisible = entity.IsVisible,
		VendorName = entity.VendorName,
	};

	#endregion
}
