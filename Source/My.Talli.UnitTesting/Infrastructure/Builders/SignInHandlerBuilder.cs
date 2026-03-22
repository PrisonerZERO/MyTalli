namespace My.Talli.UnitTesting.Infrastructure.Builders;

using Domain.Data.Interfaces;
using Domain.Handlers.Authentication;
using Domain.Models;
using Domain.Repositories;
using Lamar;
using My.Talli.UnitTesting.Infrastructure.IoC;
using My.Talli.UnitTesting.Infrastructure.Stubs;

using ENTITIES = Domain.Entities;

/// <summary>Builder</summary>
public class SignInHandlerBuilder
{
	#region <Variables>

	private readonly Container _container;

	#endregion

	#region <Properties>

	public RepositoryAdapterAsync<UserAuthenticationApple, ENTITIES.UserAuthenticationApple> AppleAuthAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<UserAuthenticationApple, ENTITIES.UserAuthenticationApple>>();

	public AppleSignInHandler AppleHandler => _container.GetInstance<AppleSignInHandler>();

	public CurrentUserServiceStub CurrentUserService => (CurrentUserServiceStub)_container.GetInstance<ICurrentUserService>();

	public EmailLookupService EmailLookupService => _container.GetInstance<EmailLookupService>();

	public RepositoryAdapterAsync<UserAuthenticationGoogle, ENTITIES.UserAuthenticationGoogle> GoogleAuthAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<UserAuthenticationGoogle, ENTITIES.UserAuthenticationGoogle>>();

	public GoogleSignInHandler GoogleHandler => _container.GetInstance<GoogleSignInHandler>();

	public RepositoryAdapterAsync<UserAuthenticationMicrosoft, ENTITIES.UserAuthenticationMicrosoft> MicrosoftAuthAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<UserAuthenticationMicrosoft, ENTITIES.UserAuthenticationMicrosoft>>();

	public MicrosoftSignInHandler MicrosoftHandler => _container.GetInstance<MicrosoftSignInHandler>();

	public RepositoryAdapterAsync<User, ENTITIES.User> UserAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<User, ENTITIES.User>>();

	public RepositoryAdapterAsync<UserRole, ENTITIES.UserRole> UserRoleAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<UserRole, ENTITIES.UserRole>>();

	#endregion

	#region <Constructors>

	public SignInHandlerBuilder()
	{
		_container = new Container(new ContainerRegistry());
	}

	#endregion
}
