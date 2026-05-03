namespace My.Talli.Domain.DI.Lamar.IoC;

using Domain.Commands.Admin;
using Domain.Commands.Billing;
using Domain.Commands.Export;
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
using Microsoft.Extensions.DependencyInjection;

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
		For<IEntityMapper<MODELS.ExpenseEtsy, ENTITIES.ExpenseEtsy>>().Use<ExpenseEtsyMapper>();
		For<IEntityMapper<MODELS.ExpenseGumroad, ENTITIES.ExpenseGumroad>>().Use<ExpenseGumroadMapper>();
		For<IEntityMapper<MODELS.ExpenseManual, ENTITIES.ExpenseManual>>().Use<ExpenseManualMapper>();
		For<IEntityMapper<MODELS.ExpenseStripe, ENTITIES.ExpenseStripe>>().Use<ExpenseStripeMapper>();
		For<IEntityMapper<MODELS.Goal, ENTITIES.Goal>>().Use<GoalMapper>();
		For<IEntityMapper<MODELS.GoalType, ENTITIES.GoalType>>().Use<GoalTypeMapper>();
		For<IEntityMapper<MODELS.Heartbeat, ENTITIES.Heartbeat>>().Use<HeartbeatMapper>();
		For<IEntityMapper<MODELS.Order, ENTITIES.Order>>().Use<OrderMapper>();
		For<IEntityMapper<MODELS.OrderItem, ENTITIES.OrderItem>>().Use<OrderItemMapper>();
		For<IEntityMapper<MODELS.Payout, ENTITIES.Payout>>().Use<PayoutMapper>();
		For<IEntityMapper<MODELS.PayoutEtsy, ENTITIES.PayoutEtsy>>().Use<PayoutEtsyMapper>();
		For<IEntityMapper<MODELS.PayoutGumroad, ENTITIES.PayoutGumroad>>().Use<PayoutGumroadMapper>();
		For<IEntityMapper<MODELS.PayoutManual, ENTITIES.PayoutManual>>().Use<PayoutManualMapper>();
		For<IEntityMapper<MODELS.PayoutStripe, ENTITIES.PayoutStripe>>().Use<PayoutStripeMapper>();
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
		For<IEntityMapper<MODELS.SystemSetting, ENTITIES.SystemSetting>>().Use<SystemSettingMapper>();
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
		For<StripeWebhookHandler>();

		// Commands — AddScoped registers with IServiceProviderIsService so Minimal API parameter binding recognizes them as DI services
		this.AddScoped<ConnectEtsyCommand>();
		this.AddScoped<ConnectGumroadCommand>();
		this.AddScoped<ConnectStripeCommand>();
		this.AddScoped<CreateManualShopCommand>();
		this.AddScoped<FindActiveSubscriptionWithStripeCommand>();
		this.AddScoped<GetExportDataCommand>();
		this.AddScoped<GetExportPreviewCommand>();
		this.AddScoped<GetSyncHealthCommand>();
		this.AddScoped<GetSystemSettingCommand>();
		this.AddScoped<RefreshShopTokensCommand>();
		this.AddScoped<RenameManualShopCommand>();
		this.AddScoped<UpdateLocalSubscriptionCommand>();
		this.AddScoped<UpdateShopSyncStateCommand>();
		this.AddScoped<UpsertEtsyExpenseCommand>();
		this.AddScoped<UpsertEtsyPayoutCommand>();
		this.AddScoped<UpsertEtsyRevenueCommand>();
		this.AddScoped<UpsertGumroadRevenueCommand>();
		this.AddScoped<UpsertStripePayoutCommand>();
		this.AddScoped<UpsertStripeRevenueCommand>();
		this.AddScoped<UpsertSystemSettingCommand>();
		this.AddScoped<WriteHeartbeatTickCommand>();
	}

	#endregion
}
