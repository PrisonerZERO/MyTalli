namespace My.Talli.Domain.Handlers.Authentication;

using Domain.Components.JsonSerializers;
using Domain.Data.Interfaces;
using Domain.Repositories;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Handler</summary>
public class MicrosoftSignInHandler
{
	#region <Variables>

	private readonly ICurrentUserService _currentUserService;
	private readonly RepositoryAdapterAsync<MODELS.UserAuthenticationMicrosoft, ENTITIES.UserAuthenticationMicrosoft> _microsoftAuthAdapter;
	private readonly RepositoryAdapterAsync<MODELS.User, ENTITIES.User> _userAdapter;
	private readonly UserPreferencesJsonSerializer _preferencesSerializer;

	#endregion

	#region <Constructors>

	public MicrosoftSignInHandler(
		ICurrentUserService currentUserService,
		RepositoryAdapterAsync<MODELS.UserAuthenticationMicrosoft, ENTITIES.UserAuthenticationMicrosoft> microsoftAuthAdapter,
		RepositoryAdapterAsync<MODELS.User, ENTITIES.User> userAdapter,
		UserPreferencesJsonSerializer preferencesSerializer)
	{
		_currentUserService = currentUserService;
		_microsoftAuthAdapter = microsoftAuthAdapter;
		_preferencesSerializer = preferencesSerializer;
		_userAdapter = userAdapter;
	}

	#endregion

	#region <Methods>

	public async Task<MODELS.User> HandleAsync(SignInArgumentOf<MicrosoftSignInPayload> argument)
	{
		var existing = (await _microsoftAuthAdapter.FindAsync(x => x.MicrosoftId == argument.Payload.MicrosoftId)).FirstOrDefault();

		if (existing is not null)
		{
			var user = (await _userAdapter.GetByIdAsync(existing.UserId))!;
			user.LastLoginAt = DateTime.UtcNow;
			return await _userAdapter.UpdateAsync(user);
		}

		return await CreateUserAsync(argument);
	}

	private async Task<MODELS.User> CreateUserAsync(SignInArgumentOf<MicrosoftSignInPayload> argument)
	{
		var defaultPreferences = _preferencesSerializer.Serialize(new MODELS.UserPreferences());

		var user = await _userAdapter.InsertAsync(new MODELS.User
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

		await _microsoftAuthAdapter.InsertAsync(new MODELS.UserAuthenticationMicrosoft
		{
			DisplayName = argument.DisplayName,
			Email = argument.Email,
			FirstName = argument.FirstName,
			LastName = argument.LastName,
			MicrosoftId = argument.Payload.MicrosoftId,
			UserId = user.Id
		});

		return user;
	}

	#endregion
}
