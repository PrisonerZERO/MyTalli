namespace My.Talli.Domain.IoC;

using Domain.Components.JsonSerializers;
using Domain.Data.EntityFramework.Repositories;
using Domain.Data.EntityFramework.Resolvers;
using Domain.Data.Interfaces;
using Domain.Handlers.Authentication;
using Domain.Mappers;
using Domain.Repositories;
using Lamar;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Registry</summary>
public class ContainerRegistry : ServiceRegistry
{
	#region <Constructors>

	public ContainerRegistry()
	{
		For(typeof(IAuditResolver<>)).Use(typeof(AuditResolver<>));
		For(typeof(IAuditableRepositoryAsync<>)).Use(typeof(GenericAuditableRepositoryAsync<>));
		For(typeof(RepositoryAdapterAsync<,>)).Use(typeof(RepositoryAdapterAsync<,>));

		For<IEntityMapper<MODELS.Billing, ENTITIES.Billing>>().Use<BillingMapper>();
		For<IEntityMapper<MODELS.BillingStripe, ENTITIES.BillingStripe>>().Use<BillingStripeMapper>();
		For<IEntityMapper<MODELS.Order, ENTITIES.Order>>().Use<OrderMapper>();
		For<IEntityMapper<MODELS.OrderItem, ENTITIES.OrderItem>>().Use<OrderItemMapper>();
		For<IEntityMapper<MODELS.Product, ENTITIES.Product>>().Use<ProductMapper>();
		For<IEntityMapper<MODELS.ProductType, ENTITIES.ProductType>>().Use<ProductTypeMapper>();
		For<IEntityMapper<MODELS.ProductVendor, ENTITIES.ProductVendor>>().Use<ProductVendorMapper>();
		For<IEntityMapper<MODELS.Subscription, ENTITIES.Subscription>>().Use<SubscriptionMapper>();
		For<IEntityMapper<MODELS.SubscriptionStripe, ENTITIES.SubscriptionStripe>>().Use<SubscriptionStripeMapper>();
		For<IEntityMapper<MODELS.Suggestion, ENTITIES.Suggestion>>().Use<SuggestionMapper>();
		For<IEntityMapper<MODELS.SuggestionVote, ENTITIES.SuggestionVote>>().Use<SuggestionVoteMapper>();
		For<IEntityMapper<MODELS.User, ENTITIES.User>>().Use<UserMapper>();
		For<IEntityMapper<MODELS.UserAuthenticationApple, ENTITIES.UserAuthenticationApple>>().Use<UserAuthenticationAppleMapper>();
		For<IEntityMapper<MODELS.UserAuthenticationGoogle, ENTITIES.UserAuthenticationGoogle>>().Use<UserAuthenticationGoogleMapper>();
		For<IEntityMapper<MODELS.UserAuthenticationMicrosoft, ENTITIES.UserAuthenticationMicrosoft>>().Use<UserAuthenticationMicrosoftMapper>();
		For<IEntityMapper<MODELS.UserRole, ENTITIES.UserRole>>().Use<UserRoleMapper>();

		For<UserPreferencesJsonSerializer>();

		For<AppleSignInHandler>();
		For<EmailLookupService>();
		For<GoogleSignInHandler>();
		For<MicrosoftSignInHandler>();
	}

	#endregion
}
