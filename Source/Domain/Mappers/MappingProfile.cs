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
        CreateMap<ENTITIES.Billing, MODELS.Billing>().ReverseMap();
        CreateMap<ENTITIES.BillingStripe, MODELS.BillingStripe>().ReverseMap();
        CreateMap<ENTITIES.Order, MODELS.Order>().ReverseMap();
        CreateMap<ENTITIES.OrderItem, MODELS.OrderItem>().ReverseMap();
        CreateMap<ENTITIES.Product, MODELS.Product>().ReverseMap();
        CreateMap<ENTITIES.ProductType, MODELS.ProductType>().ReverseMap();
        CreateMap<ENTITIES.ProductVendor, MODELS.ProductVendor>().ReverseMap();
        CreateMap<ENTITIES.Subscription, MODELS.Subscription>().ReverseMap();
        CreateMap<ENTITIES.SubscriptionStripe, MODELS.SubscriptionStripe>().ReverseMap();
        CreateMap<ENTITIES.User, MODELS.User>().ReverseMap();
        CreateMap<ENTITIES.UserAuthenticationApple, MODELS.UserAuthenticationApple>().ReverseMap();
        CreateMap<ENTITIES.UserAuthenticationGoogle, MODELS.UserAuthenticationGoogle>().ReverseMap();
        CreateMap<ENTITIES.UserAuthenticationMicrosoft, MODELS.UserAuthenticationMicrosoft>().ReverseMap();
        CreateMap<ENTITIES.UserRole, MODELS.UserRole>().ReverseMap();
    }

    #endregion
}
