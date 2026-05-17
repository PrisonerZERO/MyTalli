namespace My.Talli.UnitTesting.Infrastructure.IoC;

using Domain.Data.Interfaces;
using Lamar;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using My.Talli.UnitTesting.Infrastructure.Stubs;
using My.Talli.Web.Commands.Billing;
using My.Talli.Web.Services.Admin;
using My.Talli.Web.Services.Billing;

/// <summary>Registry</summary>
public class ContainerRegistry : ServiceRegistry
{
	#region <Constructors>

	public ContainerRegistry()
	{
		IncludeRegistry<Domain.DI.Lamar.IoC.ContainerRegistry>();

		ForSingletonOf<IdentityProvider>().Use<IdentityProvider>();
		ForSingletonOf<ICurrentUserService>().Use<CurrentUserServiceStub>();
		ForSingletonOf<IMaintenanceModeService>().Use<MaintenanceModeService>();
		ForSingletonOf<IStripeBillingApiClient>().Use<StripeBillingApiClientStub>();
		ForSingletonOf<ILogger<ReconcileBillingHealthCommand>>().Use<CapturingLogger<ReconcileBillingHealthCommand>>();

		this.AddScoped<ReconcileBillingHealthCommand>();

		For(typeof(IAuditResolver<>)).Use(typeof(AuditResolverStub<>)).Singleton();
		For(typeof(IAuditableRepositoryAsync<>)).Use(typeof(AuditableRepositoryStub<>)).Singleton();
	}

	#endregion
}
