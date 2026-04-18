namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class ShopConnectionEtsyMapper : EntityMapper<MODELS.ShopConnectionEtsy, ENTITIES.ShopConnectionEtsy>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.ShopConnectionEtsy source, MODELS.ShopConnectionEtsy destination)
	{
		destination.CountryCode = source.CountryCode;
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsVacationMode = source.IsVacationMode;
		destination.IsVisible = source.IsVisible;
		destination.ShopCurrency = source.ShopCurrency;
		destination.ShopUrl = source.ShopUrl;
	}

	public override ENTITIES.ShopConnectionEtsy ToEntity(MODELS.ShopConnectionEtsy model) => new()
	{
		CountryCode = model.CountryCode,
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsVacationMode = model.IsVacationMode,
		IsVisible = model.IsVisible,
		ShopCurrency = model.ShopCurrency,
		ShopUrl = model.ShopUrl,
	};

	public override MODELS.ShopConnectionEtsy ToModel(ENTITIES.ShopConnectionEtsy entity) => new()
	{
		CountryCode = entity.CountryCode,
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsVacationMode = entity.IsVacationMode,
		IsVisible = entity.IsVisible,
		ShopCurrency = entity.ShopCurrency,
		ShopUrl = entity.ShopUrl,
	};

	#endregion
}
