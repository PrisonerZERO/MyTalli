namespace My.Talli.Domain.Mappers;

using AutoMapper;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Profile</summary>
public class MappingProfile : Profile
{
    #region <Constructors>

    public MappingProfile()
    {
        CreateMap<ENTITIES.Billing, MODELS.Billing>();
        CreateMap<ENTITIES.BillingStripe, MODELS.BillingStripe>();
        CreateMap<ENTITIES.Order, MODELS.Order>();
        CreateMap<ENTITIES.OrderItem, MODELS.OrderItem>();
        CreateMap<ENTITIES.Product, MODELS.Product>();
        CreateMap<ENTITIES.ProductType, MODELS.ProductType>();
        CreateMap<ENTITIES.ProductVendor, MODELS.ProductVendor>();
        CreateMap<ENTITIES.Subscription, MODELS.Subscription>();
        CreateMap<ENTITIES.SubscriptionStripe, MODELS.SubscriptionStripe>();
        CreateMap<ENTITIES.User, MODELS.User>();
        CreateMap<ENTITIES.UserAuthenticationApple, MODELS.UserAuthenticationApple>();
        CreateMap<ENTITIES.UserAuthenticationGoogle, MODELS.UserAuthenticationGoogle>();
        CreateMap<ENTITIES.UserAuthenticationMicrosoft, MODELS.UserAuthenticationMicrosoft>();
    }

    #endregion
}
