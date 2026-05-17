namespace My.Talli.UnitTesting.Infrastructure.IoC;

using Domain.Components.Tokens;
using Domain.Data.Interfaces;
using Lamar;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using My.Talli.UnitTesting.Infrastructure.Stubs;
using My.Talli.Web.Commands.Billing;
using My.Talli.Web.Commands.Notifications;
using My.Talli.Web.Services.Admin;
using My.Talli.Web.Services.Billing;
using My.Talli.Web.Services.Email;

/// <summary>Registry</summary>
public class ContainerRegistry : ServiceRegistry
{
	#region <Constructors>

	public ContainerRegistry()
	{
		IncludeRegistry<Domain.DI.Lamar.IoC.ContainerRegistry>();

		ForSingletonOf<IdentityProvider>().Use<IdentityProvider>();
		ForSingletonOf<ICurrentUserService>().Use<CurrentUserServiceStub>();
		ForSingletonOf<IEmailService>().Use<EmailServiceStub>();
		ForSingletonOf<IMaintenanceModeService>().Use<MaintenanceModeService>();
		ForSingletonOf<IStripeBillingApiClient>().Use<StripeBillingApiClientStub>();
		ForSingletonOf<ILogger<NotifyExpiredSubscribersCommand>>().Use<CapturingLogger<NotifyExpiredSubscribersCommand>>();
		ForSingletonOf<ILogger<ReconcileBillingHealthCommand>>().Use<CapturingLogger<ReconcileBillingHealthCommand>>();
		ForSingletonOf<ILogger<SendSubscriptionExpiredEmailCommand>>().Use<CapturingLogger<SendSubscriptionExpiredEmailCommand>>();
		ForSingletonOf<UnsubscribeTokenService>().Use(_ => new UnsubscribeTokenService("test-secret-key-for-unit-tests-only"));

		this.AddScoped<NotifyExpiredSubscribersCommand>();
		this.AddScoped<ReconcileBillingHealthCommand>();
		this.AddScoped<SendSubscriptionExpiredEmailCommand>();

		For(typeof(IAuditResolver<>)).Use(typeof(AuditResolverStub<>)).Singleton();
		For(typeof(IAuditableRepositoryAsync<>)).Use(typeof(AuditableRepositoryStub<>)).Singleton();
	}

	#endregion
}
