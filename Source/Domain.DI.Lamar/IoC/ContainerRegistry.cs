namespace My.Talli.Domain.DI.Lamar.IoC;

using Domain.Commands.Billing;
using Domain.Commands.Platforms;
using Domain.Components.JsonSerializers;
using Domain.Data.EntityFramework.Repositories;
using Domain.Data.EntityFramework.Resolvers;
using Domain.Data.Interfaces;
using Domain.Handlers.Authentication;
using Domain.Handlers.Billing;
using Domain.Mappers;
using Domain.Repositories;
using global::Lamar;

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
		For<IEntityMapper<MODELS.Expense, ENTITIES.Expense>>().Use<ExpenseMapper>();
		For<IEntityMapper<MODELS.Goal, ENTITIES.Goal>>().Use<GoalMapper>();
		For<IEntityMapper<MODELS.GoalType, ENTITIES.GoalType>>().Use<GoalTypeMapper>();
		For<IEntityMapper<MODELS.Order, ENTITIES.Order>>().Use<OrderMapper>();
		For<IEntityMapper<MODELS.OrderItem, ENTITIES.OrderItem>>().Use<OrderItemMapper>();
		For<IEntityMapper<MODELS.Payout, ENTITIES.Payout>>().Use<PayoutMapper>();
		For<IEntityMapper<MODELS.Product, ENTITIES.Product>>().Use<ProductMapper>();
		For<IEntityMapper<MODELS.ProductType, ENTITIES.ProductType>>().Use<ProductTypeMapper>();
		For<IEntityMapper<MODELS.PlatformConnection, ENTITIES.PlatformConnection>>().Use<PlatformConnectionMapper>();
		For<IEntityMapper<MODELS.ProductVendor, ENTITIES.ProductVendor>>().Use<ProductVendorMapper>();
		For<IEntityMapper<MODELS.Revenue, ENTITIES.Revenue>>().Use<RevenueMapper>();
		For<IEntityMapper<MODELS.RevenueEtsy, ENTITIES.RevenueEtsy>>().Use<RevenueEtsyMapper>();
		For<IEntityMapper<MODELS.RevenueGumroad, ENTITIES.RevenueGumroad>>().Use<RevenueGumroadMapper>();
		For<IEntityMapper<MODELS.RevenueManual, ENTITIES.RevenueManual>>().Use<RevenueManualMapper>();
		For<IEntityMapper<MODELS.RevenueStripe, ENTITIES.RevenueStripe>>().Use<RevenueStripeMapper>();
		For<IEntityMapper<MODELS.ShopConnection, ENTITIES.ShopConnection>>().Use<ShopConnectionMapper>();
		For<IEntityMapper<MODELS.ShopConnectionEtsy, ENTITIES.ShopConnectionEtsy>>().Use<ShopConnectionEtsyMapper>();
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
		For<ConnectEtsyCommand>();
		For<EmailLookupService>();
		For<FindActiveSubscriptionWithStripeCommand>();
		For<GoogleSignInHandler>();
		For<MicrosoftSignInHandler>();
		For<StripeWebhookHandler>();
		For<UpdateLocalSubscriptionCommand>();
	}

	#endregion
}
