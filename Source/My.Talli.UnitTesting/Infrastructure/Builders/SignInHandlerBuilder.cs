namespace My.Talli.UnitTesting.Infrastructure.Builders;

using Domain.Components.JsonSerializers;
using Domain.Handlers.Authentication;
using Domain.Mappers;
using Domain.Repositories;
using My.Talli.UnitTesting.Infrastructure.Stubs;

using ENTITIES = Domain.Entities;

/// <summary>Builder</summary>
public class SignInHandlerBuilder
{
	#region <Properties>

	public AuditableRepositoryStub<ENTITIES.UserAuthenticationApple> AppleAuthRepository { get; }

	public AppleSignInHandler AppleHandler { get; }

	public CurrentUserServiceStub CurrentUserService { get; }

	public EmailLookupService EmailLookupService { get; }

	public AuditableRepositoryStub<ENTITIES.UserAuthenticationGoogle> GoogleAuthRepository { get; }

	public GoogleSignInHandler GoogleHandler { get; }

	public AuditableRepositoryStub<ENTITIES.UserAuthenticationMicrosoft> MicrosoftAuthRepository { get; }

	public MicrosoftSignInHandler MicrosoftHandler { get; }

	public AuditableRepositoryStub<ENTITIES.UserRole> UserRoleRepository { get; }

	public AuditableRepositoryStub<ENTITIES.User> UserRepository { get; }

	#endregion

	#region <Constructors>

	public SignInHandlerBuilder()
	{
		CurrentUserService = new CurrentUserServiceStub();

		var userAuditResolver = new AuditResolverStub<ENTITIES.User>(CurrentUserService);
		var googleAuthAuditResolver = new AuditResolverStub<ENTITIES.UserAuthenticationGoogle>(CurrentUserService);
		var appleAuthAuditResolver = new AuditResolverStub<ENTITIES.UserAuthenticationApple>(CurrentUserService);
		var microsoftAuthAuditResolver = new AuditResolverStub<ENTITIES.UserAuthenticationMicrosoft>(CurrentUserService);
		var userRoleAuditResolver = new AuditResolverStub<ENTITIES.UserRole>(CurrentUserService);

		var identityProvider = new IdentityProvider();

		AppleAuthRepository = new AuditableRepositoryStub<ENTITIES.UserAuthenticationApple>(appleAuthAuditResolver, identityProvider);
		GoogleAuthRepository = new AuditableRepositoryStub<ENTITIES.UserAuthenticationGoogle>(googleAuthAuditResolver, identityProvider);
		MicrosoftAuthRepository = new AuditableRepositoryStub<ENTITIES.UserAuthenticationMicrosoft>(microsoftAuthAuditResolver, identityProvider);
		UserRepository = new AuditableRepositoryStub<ENTITIES.User>(userAuditResolver, identityProvider);
		UserRoleRepository = new AuditableRepositoryStub<ENTITIES.UserRole>(userRoleAuditResolver, identityProvider);

		var userMapper = new UserMapper();
		var googleAuthMapper = new UserAuthenticationGoogleMapper();
		var appleAuthMapper = new UserAuthenticationAppleMapper();
		var microsoftAuthMapper = new UserAuthenticationMicrosoftMapper();
		var userRoleMapper = new UserRoleMapper();

		var userAdapter = new RepositoryAdapterAsync<Domain.Models.User, ENTITIES.User>(UserRepository, userMapper);
		var googleAuthAdapter = new RepositoryAdapterAsync<Domain.Models.UserAuthenticationGoogle, ENTITIES.UserAuthenticationGoogle>(GoogleAuthRepository, googleAuthMapper);
		var appleAuthAdapter = new RepositoryAdapterAsync<Domain.Models.UserAuthenticationApple, ENTITIES.UserAuthenticationApple>(AppleAuthRepository, appleAuthMapper);
		var microsoftAuthAdapter = new RepositoryAdapterAsync<Domain.Models.UserAuthenticationMicrosoft, ENTITIES.UserAuthenticationMicrosoft>(MicrosoftAuthRepository, microsoftAuthMapper);
		var userRoleAdapter = new RepositoryAdapterAsync<Domain.Models.UserRole, ENTITIES.UserRole>(UserRoleRepository, userRoleMapper);

		var serializer = new UserPreferencesJsonSerializer();

		EmailLookupService = new EmailLookupService(appleAuthAdapter, googleAuthAdapter, microsoftAuthAdapter);

		AppleHandler = new AppleSignInHandler(EmailLookupService, CurrentUserService, userAdapter, appleAuthAdapter, userRoleAdapter, serializer);
		GoogleHandler = new GoogleSignInHandler(EmailLookupService, CurrentUserService, userAdapter, googleAuthAdapter, userRoleAdapter, serializer);
		MicrosoftHandler = new MicrosoftSignInHandler(EmailLookupService, CurrentUserService, userAdapter, microsoftAuthAdapter, userRoleAdapter, serializer);
	}

	#endregion
}
