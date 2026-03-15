namespace My.Talli.Domain.Mappers;

using AutoMapper;
using Domain.Models;

using ENTITIES = Domain.Entities;

/// <summary>Profile</summary>
public class MappingProfile : Profile
{
    #region <Constructors>

    public MappingProfile()
    {
        CreateMap<ENTITIES.Billing, BillingModel>();
        CreateMap<ENTITIES.BillingStripe, BillingStripeModel>();
        CreateMap<ENTITIES.Order, OrderModel>();
        CreateMap<ENTITIES.OrderItem, OrderItemModel>();
        CreateMap<ENTITIES.Product, ProductModel>();
        CreateMap<ENTITIES.ProductType, ProductTypeModel>();
        CreateMap<ENTITIES.ProductVendor, ProductVendorModel>();
        CreateMap<ENTITIES.Subscription, SubscriptionModel>();
        CreateMap<ENTITIES.SubscriptionStripe, SubscriptionStripeModel>();
        CreateMap<ENTITIES.User, UserModel>();
        CreateMap<ENTITIES.UserAuthenticationApple, UserAuthenticationAppleModel>();
        CreateMap<ENTITIES.UserAuthenticationGoogle, UserAuthenticationGoogleModel>();
        CreateMap<ENTITIES.UserAuthenticationMicrosoft, UserAuthenticationMicrosoftModel>();
    }

    #endregion
}
