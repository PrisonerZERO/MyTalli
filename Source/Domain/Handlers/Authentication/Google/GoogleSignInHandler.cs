namespace My.Talli.Domain.Handlers.Authentication;

using Domain.Components.JsonSerializers;
using Domain.Data.Interfaces;
using Domain.Framework;
using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Handler</summary>
public class GoogleSignInHandler
{
	#region <Variables>

	private readonly ICurrentUserService _currentUserService;
	private readonly RepositoryAdapterAsync<User, ENTITIES.User> _userAdapter;
	private readonly RepositoryAdapterAsync<UserAuthenticationGoogle, ENTITIES.UserAuthenticationGoogle> _googleAuthAdapter;
	private readonly RepositoryAdapterAsync<UserRole, ENTITIES.UserRole> _userRoleAdapter;
	private readonly UserPreferencesJsonSerializer _preferencesSerializer;

	#endregion

	#region <Constructors>

	public GoogleSignInHandler(
		ICurrentUserService currentUserService,
		RepositoryAdapterAsync<User, ENTITIES.User> userAdapter,
		RepositoryAdapterAsync<UserAuthenticationGoogle, ENTITIES.UserAuthenticationGoogle> googleAuthAdapter,
		RepositoryAdapterAsync<UserRole, ENTITIES.UserRole> userRoleAdapter,
		UserPreferencesJsonSerializer preferencesSerializer)
	{
		_currentUserService = currentUserService;
		_googleAuthAdapter = googleAuthAdapter;
		_preferencesSerializer = preferencesSerializer;
		_userAdapter = userAdapter;
		_userRoleAdapter = userRoleAdapter;
	}

	#endregion

	#region <Methods>

	public async Task<User> HandleAsync(SignInArgumentOf<GoogleSignInPayload> argument)
	{
		var existing = (await _googleAuthAdapter.FindAsync(x => x.GoogleId == argument.Payload.GoogleId)).FirstOrDefault();

		if (existing is not null)
		{
			var user = (await _userAdapter.GetByIdAsync(existing.Id))!;
			user.LastLoginAt = DateTime.UtcNow;
			user = await _userAdapter.UpdateAsync(user);
			user.Roles = await ResolveRolesAsync(user.Id);
			return user;
		}

		return await CreateUserAsync(argument);
	}

	private async Task<User> CreateUserAsync(SignInArgumentOf<GoogleSignInPayload> argument)
	{
		var defaultPreferences = _preferencesSerializer.Serialize(new UserPreferences());

		var user = await _userAdapter.InsertAsync(new User
		{
			DisplayName = argument.DisplayName,
			FirstName = argument.FirstName,
			InitialProvider = "Google",
			LastLoginAt = DateTime.UtcNow,
			LastName = argument.LastName,
			PreferredProvider = "Google",
			UserPreferences = defaultPreferences
		});

		_currentUserService.Set(user.Id, user.DisplayName);

		await _googleAuthAdapter.InsertAsync(new UserAuthenticationGoogle
		{
			AvatarUrl = argument.Payload.AvatarUrl,
			DisplayName = argument.DisplayName,
			Email = argument.Email,
			EmailVerified = argument.Payload.EmailVerified,
			FirstName = argument.FirstName,
			GoogleId = argument.Payload.GoogleId,
			Id = user.Id,
			LastName = argument.LastName,
			Locale = argument.Payload.Locale
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
