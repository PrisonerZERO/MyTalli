namespace My.Talli.UnitTesting.Infrastructure.IoC;

using Domain.Components.JsonSerializers;
using Domain.Data.Interfaces;
using Domain.Handlers.Authentication;
using Domain.Mappers;
using Domain.Repositories;
using Lamar;
using My.Talli.UnitTesting.Infrastructure.Stubs;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Registry</summary>
public class ContainerRegistry : ServiceRegistry
{
	#region <Constructors>

	public ContainerRegistry()
	{
		ForSingletonOf<IdentityProvider>().Use<IdentityProvider>();
		ForSingletonOf<ICurrentUserService>().Use<CurrentUserServiceStub>();

		For(typeof(IAuditResolver<>)).Use(typeof(AuditResolverStub<>)).Singleton();
		For(typeof(IAuditableRepositoryAsync<>)).Use(typeof(AuditableRepositoryStub<>)).Singleton();
		For(typeof(RepositoryAdapterAsync<,>)).Use(typeof(RepositoryAdapterAsync<,>)).Singleton();

		ForSingletonOf<IEntityMapper<MODELS.User, ENTITIES.User>>().Use<UserMapper>();
		ForSingletonOf<IEntityMapper<MODELS.UserAuthenticationApple, ENTITIES.UserAuthenticationApple>>().Use<UserAuthenticationAppleMapper>();
		ForSingletonOf<IEntityMapper<MODELS.UserAuthenticationGoogle, ENTITIES.UserAuthenticationGoogle>>().Use<UserAuthenticationGoogleMapper>();
		ForSingletonOf<IEntityMapper<MODELS.UserAuthenticationMicrosoft, ENTITIES.UserAuthenticationMicrosoft>>().Use<UserAuthenticationMicrosoftMapper>();
		ForSingletonOf<IEntityMapper<MODELS.UserRole, ENTITIES.UserRole>>().Use<UserRoleMapper>();

		ForSingletonOf<UserPreferencesJsonSerializer>().Use<UserPreferencesJsonSerializer>();

		ForSingletonOf<AppleSignInHandler>().Use<AppleSignInHandler>();
		ForSingletonOf<EmailLookupService>().Use<EmailLookupService>();
		ForSingletonOf<GoogleSignInHandler>().Use<GoogleSignInHandler>();
		ForSingletonOf<MicrosoftSignInHandler>().Use<MicrosoftSignInHandler>();
	}

	#endregion
}
