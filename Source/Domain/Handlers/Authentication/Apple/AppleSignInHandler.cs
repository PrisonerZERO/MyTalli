namespace My.Talli.Domain.Handlers.Authentication;

using Domain.Components.JsonSerializers;
using Domain.Data.Interfaces;
using Domain.Repositories;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Handler</summary>
public class AppleSignInHandler
{
	#region <Variables>

	private readonly ICurrentUserService _currentUserService;
	private readonly RepositoryAdapterAsync<MODELS.UserAuthenticationApple, ENTITIES.UserAuthenticationApple> _appleAuthAdapter;
	private readonly RepositoryAdapterAsync<MODELS.User, ENTITIES.User> _userAdapter;
	private readonly UserPreferencesJsonSerializer _preferencesSerializer;

	#endregion

	#region <Constructors>

	public AppleSignInHandler(
		ICurrentUserService currentUserService,
		RepositoryAdapterAsync<MODELS.UserAuthenticationApple, ENTITIES.UserAuthenticationApple> appleAuthAdapter,
		RepositoryAdapterAsync<MODELS.User, ENTITIES.User> userAdapter,
		UserPreferencesJsonSerializer preferencesSerializer)
	{
		_appleAuthAdapter = appleAuthAdapter;
		_currentUserService = currentUserService;
		_preferencesSerializer = preferencesSerializer;
		_userAdapter = userAdapter;
	}

	#endregion

	#region <Methods>

	public async Task<MODELS.User> HandleAsync(SignInArgumentOf<AppleSignInPayload> argument)
	{
		var existing = (await _appleAuthAdapter.FindAsync(x => x.AppleId == argument.Payload.AppleId)).FirstOrDefault();

		if (existing is not null)
		{
			var user = (await _userAdapter.GetByIdAsync(existing.UserId))!;
			user.LastLoginAt = DateTime.UtcNow;
			return await _userAdapter.UpdateAsync(user);
		}

		return await CreateUserAsync(argument);
	}

	private async Task<MODELS.User> CreateUserAsync(SignInArgumentOf<AppleSignInPayload> argument)
	{
		var defaultPreferences = _preferencesSerializer.Serialize(new MODELS.UserPreferences());

		var user = await _userAdapter.InsertAsync(new MODELS.User
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

		await _appleAuthAdapter.InsertAsync(new MODELS.UserAuthenticationApple
		{
			AppleId = argument.Payload.AppleId,
			DisplayName = argument.DisplayName,
			Email = argument.Email,
			FirstName = argument.FirstName,
			IsPrivateRelay = argument.Payload.IsPrivateRelay,
			LastName = argument.LastName,
			UserId = user.Id
		});

		return user;
	}

	#endregion
}
