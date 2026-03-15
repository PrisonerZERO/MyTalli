namespace My.Talli.Domain.Handlers.Authentication;

using Domain.Components.JsonSerializers;
using Domain.Data.Interfaces;
using Domain.Repositories;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Handler</summary>
public class GoogleSignInHandler
{
	#region <Variables>

	private readonly ICurrentUserService _currentUserService;
	private readonly RepositoryAdapterAsync<MODELS.UserAuthenticationGoogle, ENTITIES.UserAuthenticationGoogle> _googleAuthAdapter;
	private readonly RepositoryAdapterAsync<MODELS.User, ENTITIES.User> _userAdapter;
	private readonly UserPreferencesJsonSerializer _preferencesSerializer;

	#endregion

	#region <Constructors>

	public GoogleSignInHandler(
		ICurrentUserService currentUserService,
		RepositoryAdapterAsync<MODELS.UserAuthenticationGoogle, ENTITIES.UserAuthenticationGoogle> googleAuthAdapter,
		RepositoryAdapterAsync<MODELS.User, ENTITIES.User> userAdapter,
		UserPreferencesJsonSerializer preferencesSerializer)
	{
		_currentUserService = currentUserService;
		_googleAuthAdapter = googleAuthAdapter;
		_preferencesSerializer = preferencesSerializer;
		_userAdapter = userAdapter;
	}

	#endregion

	#region <Methods>

	public async Task<MODELS.User> HandleAsync(SignInArgumentOf<GoogleSignInPayload> argument)
	{
		var existing = (await _googleAuthAdapter.FindAsync(x => x.GoogleId == argument.Payload.GoogleId)).FirstOrDefault();

		if (existing is not null)
		{
			var user = (await _userAdapter.GetByIdAsync(existing.UserId))!;
			user.LastLoginAt = DateTime.UtcNow;
			return await _userAdapter.UpdateAsync(user);
		}

		return await CreateUserAsync(argument);
	}

	private async Task<MODELS.User> CreateUserAsync(SignInArgumentOf<GoogleSignInPayload> argument)
	{
		var defaultPreferences = _preferencesSerializer.Serialize(new MODELS.UserPreferences());

		var user = await _userAdapter.InsertAsync(new MODELS.User
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

		await _googleAuthAdapter.InsertAsync(new MODELS.UserAuthenticationGoogle
		{
			AvatarUrl = argument.Payload.AvatarUrl,
			DisplayName = argument.DisplayName,
			Email = argument.Email,
			EmailVerified = argument.Payload.EmailVerified,
			FirstName = argument.FirstName,
			GoogleId = argument.Payload.GoogleId,
			LastName = argument.LastName,
			Locale = argument.Payload.Locale,
			UserId = user.Id
		});

		return user;
	}

	#endregion
}
