namespace My.Talli.UnitTesting.Infrastructure.IoC;

using Domain.Components.Tokens;
using Domain.Data.Interfaces;
using Lamar;
using My.Talli.UnitTesting.Infrastructure.Stubs;
using My.Talli.Web.Services.Admin;

/// <summary>Registry</summary>
public class ContainerRegistry : ServiceRegistry
{
	#region <Constructors>

	public ContainerRegistry()
	{
		IncludeRegistry<Domain.DI.Lamar.IoC.ContainerRegistry>();

		ForSingletonOf<IdentityProvider>().Use<IdentityProvider>();
		ForSingletonOf<ICurrentUserService>().Use<CurrentUserServiceStub>();
		ForSingletonOf<IShopTokenProtector>().Use<ShopTokenProtectorStub>();
		ForSingletonOf<IMaintenanceModeService>().Use<MaintenanceModeService>();

		For(typeof(IAuditResolver<>)).Use(typeof(AuditResolverStub<>)).Singleton();
		For(typeof(IAuditableRepositoryAsync<>)).Use(typeof(AuditableRepositoryStub<>)).Singleton();
	}

	#endregion
}
