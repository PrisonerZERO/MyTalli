namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class ExpenseEtsyMapper : EntityMapper<MODELS.ExpenseEtsy, ENTITIES.ExpenseEtsy>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.ExpenseEtsy source, MODELS.ExpenseEtsy destination)
	{
		destination.AdCampaignId = source.AdCampaignId;
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsVisible = source.IsVisible;
		destination.LedgerEntryId = source.LedgerEntryId;
		destination.ListingId = source.ListingId;
	}

	public override ENTITIES.ExpenseEtsy ToEntity(MODELS.ExpenseEtsy model) => new()
	{
		AdCampaignId = model.AdCampaignId,
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsVisible = model.IsVisible,
		LedgerEntryId = model.LedgerEntryId,
		ListingId = model.ListingId,
	};

	public override MODELS.ExpenseEtsy ToModel(ENTITIES.ExpenseEtsy entity) => new()
	{
		AdCampaignId = entity.AdCampaignId,
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsVisible = entity.IsVisible,
		LedgerEntryId = entity.LedgerEntryId,
		ListingId = entity.ListingId,
	};

	#endregion
}
