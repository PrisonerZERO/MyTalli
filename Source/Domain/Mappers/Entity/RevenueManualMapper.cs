namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class RevenueManualMapper : EntityMapper<MODELS.RevenueManual, ENTITIES.RevenueManual>
{
    #region <Methods>

    public override void ApplyTo(ENTITIES.RevenueManual source, MODELS.RevenueManual destination)
    {
        destination.Category = source.Category;
        destination.Id = source.Id;
        destination.IsDeleted = source.IsDeleted;
        destination.IsVisible = source.IsVisible;
        destination.Notes = source.Notes;
        destination.Quantity = source.Quantity;
    }

    public override ENTITIES.RevenueManual ToEntity(MODELS.RevenueManual model) => new()
    {
        Category = model.Category,
        Id = model.Id,
        IsDeleted = model.IsDeleted,
        IsVisible = model.IsVisible,
        Notes = model.Notes,
        Quantity = model.Quantity,
    };

    public override MODELS.RevenueManual ToModel(ENTITIES.RevenueManual entity) => new()
    {
        Category = entity.Category,
        Id = entity.Id,
        IsDeleted = entity.IsDeleted,
        IsVisible = entity.IsVisible,
        Notes = entity.Notes,
        Quantity = entity.Quantity,
    };

    #endregion
}
