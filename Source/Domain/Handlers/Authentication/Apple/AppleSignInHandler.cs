namespace My.Talli.Domain.Handlers.Authentication;

using Domain.Components.JsonSerializers;
using Domain.Data.Interfaces;
using Domain.Framework;
using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Handler</summary>
public class AppleSignInHandler
{
	#region <Variables>

	private readonly ICurrentUserService _currentUserService;
	private readonly RepositoryAdapterAsync<User, ENTITIES.User> _userAdapter;
	private readonly RepositoryAdapterAsync<UserAuthenticationApple, ENTITIES.UserAuthenticationApple> _appleAuthAdapter;
	private readonly RepositoryAdapterAsync<UserRole, ENTITIES.UserRole> _userRoleAdapter;
	private readonly UserPreferencesJsonSerializer _preferencesSerializer;

	#endregion

	#region <Constructors>

	public AppleSignInHandler(
		ICurrentUserService currentUserService,
		RepositoryAdapterAsync<User, ENTITIES.User> userAdapter,
		RepositoryAdapterAsync<UserAuthenticationApple, ENTITIES.UserAuthenticationApple> appleAuthAdapter,
		RepositoryAdapterAsync<UserRole, ENTITIES.UserRole> userRoleAdapter,
		UserPreferencesJsonSerializer preferencesSerializer)
	{
		_appleAuthAdapter = appleAuthAdapter;
		_currentUserService = currentUserService;
		_preferencesSerializer = preferencesSerializer;
		_userAdapter = userAdapter;
		_userRoleAdapter = userRoleAdapter;
	}

	#endregion

	#region <Methods>

	public async Task<User> HandleAsync(SignInArgumentOf<AppleSignInPayload> argument)
	{
		var existing = (await _appleAuthAdapter.FindAsync(x => x.AppleId == argument.Payload.AppleId)).FirstOrDefault();

		if (existing is not null)
		{
			var user = (await _userAdapter.GetByIdAsync(existing.UserId))!;
			user.LastLoginAt = DateTime.UtcNow;
			user = await _userAdapter.UpdateAsync(user);
			user.Roles = await ResolveRolesAsync(user.Id);
			return user;
		}

		return await CreateUserAsync(argument);
	}

	private async Task<User> CreateUserAsync(SignInArgumentOf<AppleSignInPayload> argument)
	{
		var defaultPreferences = _preferencesSerializer.Serialize(new UserPreferences());

		var user = await _userAdapter.InsertAsync(new User
		{
			DisplayName = argument.DisplayName,
			FirstName = argument.FirstName,
			InitialProvider = "Apple",
			LastLoginAt = DateTime.UtcNow,
			LastName = argument.LastName,
			PreferredProvider = "Apple",
			UserPreferences = defaultPreferences
		});

		_currentUserService.Set(user.Id, user.DisplayName);

		await _appleAuthAdapter.InsertAsync(new UserAuthenticationApple
		{
			AppleId = argument.Payload.AppleId,
			DisplayName = argument.DisplayName,
			Email = argument.Email,
			FirstName = argument.FirstName,
			IsPrivateRelay = argument.Payload.IsPrivateRelay,
			LastName = argument.LastName,
			UserId = user.Id
		});

		await _userRoleAdapter.InsertAsync(new UserRole { Role = Roles.User, UserId = user.Id });
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
