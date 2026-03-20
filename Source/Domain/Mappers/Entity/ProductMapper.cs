namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class ProductMapper : EntityMapper<MODELS.Product, ENTITIES.Product>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.Product source, MODELS.Product destination)
	{
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsVisible = source.IsVisible;
		destination.ProductName = source.ProductName;
		destination.ProductTypeId = source.ProductTypeId;
		destination.VendorId = source.VendorId;
		destination.VendorPrice = source.VendorPrice;
	}

	public override ENTITIES.Product ToEntity(MODELS.Product model) => new()
	{
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsVisible = model.IsVisible,
		ProductName = model.ProductName,
		ProductTypeId = model.ProductTypeId,
		VendorId = model.VendorId,
		VendorPrice = model.VendorPrice,
	};

	public override MODELS.Product ToModel(ENTITIES.Product entity) => new()
	{
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsVisible = entity.IsVisible,
		ProductName = entity.ProductName,
		ProductTypeId = entity.ProductTypeId,
		VendorId = entity.VendorId,
		VendorPrice = entity.VendorPrice,
	};


	#endregion
}
