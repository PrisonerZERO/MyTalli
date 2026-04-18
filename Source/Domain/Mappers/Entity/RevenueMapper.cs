namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class RevenueMapper : EntityMapper<MODELS.Revenue, ENTITIES.Revenue>
{
    #region <Methods>

    public override void ApplyTo(ENTITIES.Revenue source, MODELS.Revenue destination)
    {
        destination.Currency = source.Currency;
        destination.Description = source.Description;
        destination.FeeAmount = source.FeeAmount;
        destination.GrossAmount = source.GrossAmount;
        destination.Id = source.Id;
        destination.IsDeleted = source.IsDeleted;
        destination.IsDisputed = source.IsDisputed;
        destination.IsRefunded = source.IsRefunded;
        destination.IsVisible = source.IsVisible;
        destination.NetAmount = source.NetAmount;
        destination.Platform = source.Platform;
        destination.PlatformTransactionId = source.PlatformTransactionId;
        destination.ShopConnectionId = source.ShopConnectionId;
        destination.TransactionDate = source.TransactionDate;
        destination.UserId = source.UserId;
    }

    public override ENTITIES.Revenue ToEntity(MODELS.Revenue model) => new()
    {
        Currency = model.Currency,
        Description = model.Description,
        FeeAmount = model.FeeAmount,
        GrossAmount = model.GrossAmount,
        Id = model.Id,
        IsDeleted = model.IsDeleted,
        IsDisputed = model.IsDisputed,
        IsRefunded = model.IsRefunded,
        IsVisible = model.IsVisible,
        NetAmount = model.NetAmount,
        Platform = model.Platform,
        PlatformTransactionId = model.PlatformTransactionId,
        ShopConnectionId = model.ShopConnectionId,
        TransactionDate = model.TransactionDate,
        UserId = model.UserId,
    };

    public override MODELS.Revenue ToModel(ENTITIES.Revenue entity) => new()
    {
        Currency = entity.Currency,
        Description = entity.Description,
        FeeAmount = entity.FeeAmount,
        GrossAmount = entity.GrossAmount,
        Id = entity.Id,
        IsDeleted = entity.IsDeleted,
        IsDisputed = entity.IsDisputed,
        IsRefunded = entity.IsRefunded,
        IsVisible = entity.IsVisible,
        NetAmount = entity.NetAmount,
        Platform = entity.Platform,
        PlatformTransactionId = entity.PlatformTransactionId,
        ShopConnectionId = entity.ShopConnectionId,
        TransactionDate = entity.TransactionDate,
        UserId = entity.UserId,
    };

    #endregion
}
