namespace My.Talli.Web.Configuration;

using Domain.Components.JsonSerializers;
using Domain.Data.EntityFramework.Repositories;
using Domain.Data.EntityFramework.Resolvers;
using Domain.Data.Interfaces;
using Domain.Handlers.Authentication;
using Domain.Mappers;
using Domain.Repositories;
using Web.Services.Identity;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Configuration</summary>
public static class RepositoryConfiguration
{
    #region <Methods>

    public static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped(typeof(IAuditResolver<>), typeof(AuditResolver<>));
        services.AddScoped(typeof(IAuditableRepositoryAsync<>), typeof(GenericAuditableRepositoryAsync<>));
        services.AddScoped(typeof(RepositoryAdapterAsync<,>));
        services.AddScoped<IEntityMapper<MODELS.Billing, ENTITIES.Billing>, BillingMapper>();
        services.AddScoped<IEntityMapper<MODELS.BillingStripe, ENTITIES.BillingStripe>, BillingStripeMapper>();
        services.AddScoped<IEntityMapper<MODELS.Milestone, ENTITIES.Milestone>, MilestoneMapper>();
        services.AddScoped<IEntityMapper<MODELS.Order, ENTITIES.Order>, OrderMapper>();
        services.AddScoped<IEntityMapper<MODELS.OrderItem, ENTITIES.OrderItem>, OrderItemMapper>();
        services.AddScoped<IEntityMapper<MODELS.Product, ENTITIES.Product>, ProductMapper>();
        services.AddScoped<IEntityMapper<MODELS.ProductType, ENTITIES.ProductType>, ProductTypeMapper>();
        services.AddScoped<IEntityMapper<MODELS.ProductVendor, ENTITIES.ProductVendor>, ProductVendorMapper>();
        services.AddScoped<IEntityMapper<MODELS.Subscription, ENTITIES.Subscription>, SubscriptionMapper>();
        services.AddScoped<IEntityMapper<MODELS.SubscriptionStripe, ENTITIES.SubscriptionStripe>, SubscriptionStripeMapper>();
        services.AddScoped<IEntityMapper<MODELS.User, ENTITIES.User>, UserMapper>();
        services.AddScoped<IEntityMapper<MODELS.UserAuthenticationApple, ENTITIES.UserAuthenticationApple>, UserAuthenticationAppleMapper>();
        services.AddScoped<IEntityMapper<MODELS.UserAuthenticationGoogle, ENTITIES.UserAuthenticationGoogle>, UserAuthenticationGoogleMapper>();
        services.AddScoped<IEntityMapper<MODELS.UserAuthenticationMicrosoft, ENTITIES.UserAuthenticationMicrosoft>, UserAuthenticationMicrosoftMapper>();
        services.AddScoped<IEntityMapper<MODELS.UserRole, ENTITIES.UserRole>, UserRoleMapper>();
        services.AddScoped<UserPreferencesJsonSerializer>();
        services.AddScoped<AppleSignInHandler>();
        services.AddScoped<EmailLookupService>();
        services.AddScoped<GoogleSignInHandler>();
        services.AddScoped<MicrosoftSignInHandler>();
    }

    #endregion
}
