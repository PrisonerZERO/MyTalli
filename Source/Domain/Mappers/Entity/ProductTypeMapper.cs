namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class ProductTypeMapper : EntityMapperBase<MODELS.ProductType, ENTITIES.ProductType>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.ProductType source, MODELS.ProductType destination)
	{
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsVisible = source.IsVisible;
		destination.ProductTypeName = source.ProductTypeName;
	}

	public override ENTITIES.ProductType ToEntity(MODELS.ProductType model) => new()
	{
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsVisible = model.IsVisible,
		ProductTypeName = model.ProductTypeName,
	};

	public override MODELS.ProductType ToModel(ENTITIES.ProductType entity) => new()
	{
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsVisible = entity.IsVisible,
		ProductTypeName = entity.ProductTypeName,
	};

	#endregion
}
