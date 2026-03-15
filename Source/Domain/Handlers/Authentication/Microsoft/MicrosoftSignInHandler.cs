namespace My.Talli.Domain.Handlers.Authentication;

using Domain.Components.JsonSerializers;
using Domain.Data.Interfaces;
using Domain.Framework;
using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Handler</summary>
public class MicrosoftSignInHandler
{
	#region <Variables>

	private readonly ICurrentUserService _currentUserService;
	private readonly RepositoryAdapterAsync<User, ENTITIES.User> _userAdapter;
	private readonly RepositoryAdapterAsync<UserAuthenticationMicrosoft, ENTITIES.UserAuthenticationMicrosoft> _microsoftAuthAdapter;
	private readonly RepositoryAdapterAsync<UserRole, ENTITIES.UserRole> _userRoleAdapter;
	private readonly UserPreferencesJsonSerializer _preferencesSerializer;

	#endregion

	#region <Constructors>

	public MicrosoftSignInHandler(
		ICurrentUserService currentUserService,
		RepositoryAdapterAsync<User, ENTITIES.User> userAdapter,
		RepositoryAdapterAsync<UserAuthenticationMicrosoft, ENTITIES.UserAuthenticationMicrosoft> microsoftAuthAdapter,
		RepositoryAdapterAsync<UserRole, ENTITIES.UserRole> userRoleAdapter,
		UserPreferencesJsonSerializer preferencesSerializer)
	{
		_currentUserService = currentUserService;
		_microsoftAuthAdapter = microsoftAuthAdapter;
		_preferencesSerializer = preferencesSerializer;
		_userAdapter = userAdapter;
		_userRoleAdapter = userRoleAdapter;
	}

	#endregion

	#region <Methods>

	public async Task<User> HandleAsync(SignInArgumentOf<MicrosoftSignInPayload> argument)
	{
		var existing = (await _microsoftAuthAdapter.FindAsync(x => x.MicrosoftId == argument.Payload.MicrosoftId)).FirstOrDefault();

		if (existing is not null)
		{
			var user = (await _userAdapter.GetByIdAsync(existing.Id))!;
			_currentUserService.Set(user.Id, user.DisplayName);
			user.LastLoginAt = DateTime.UtcNow;
			user = await _userAdapter.UpdateAsync(user);
			user.Roles = await ResolveRolesAsync(user.Id);
			return user;
		}

		return await CreateUserAsync(argument);
	}

	private async Task<User> CreateUserAsync(SignInArgumentOf<MicrosoftSignInPayload> argument)
	{
		var defaultPreferences = _preferencesSerializer.Serialize(new UserPreferences());

		var user = await _userAdapter.InsertAsync(new User
		{
			DisplayName = argument.DisplayName,
			FirstName = argument.FirstName,
			InitialProvider = "Microsoft",
			LastLoginAt = DateTime.UtcNow,
			LastName = argument.LastName,
			PreferredProvider = "Microsoft",
			UserPreferences = defaultPreferences
		});

		_currentUserService.Set(user.Id, user.DisplayName);

		await _microsoftAuthAdapter.InsertAsync(new UserAuthenticationMicrosoft
		{
			DisplayName = argument.DisplayName,
			Email = argument.Email,
			FirstName = argument.FirstName,
			Id = user.Id,
			LastName = argument.LastName,
			MicrosoftId = argument.Payload.MicrosoftId
		});

		await _userRoleAdapter.InsertAsync(new UserRole { Role = Roles.User, UserId = user.Id });
		user.IsNewUser = true;
		user.Roles = [Roles.User];

		return user;
	}

	private async Task<List<string>> ResolveRolesAsync(long userId)
	{
		var roles = (await _userRoleAdapter.FindAsync(x => x.UserId == userId)).Select(r => r.Role).ToList();

		if (roles.Count == 0)
		{
			await _userRoleAdapter.InsertAsync(new UserRole { Role = Roles.User, UserId = userId });
			roles = [Roles.User];
		}

		return roles;
	}

	#endregion
}
