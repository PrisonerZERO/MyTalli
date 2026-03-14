namespace My.Talli.Domain.Handlers.Authentication;

using Domain.Components.JsonSerializers;
using Domain.Data.Interfaces;
using Domain.Models;

using ENTITIES = Domain.Entities;

/// <summary>Handler</summary>
public class GoogleSignInHandler
{
	#region <Variables>

	private readonly IAuditableRepositoryAsync<ENTITIES.UserAuthenticationGoogle> _googleAuthRepository;
	private readonly UserPreferencesJsonSerializer _preferencesSerializer;
	private readonly IAuditableRepositoryAsync<ENTITIES.User> _userRepository;

	#endregion

	#region <Constructors>

	public GoogleSignInHandler(
		IAuditableRepositoryAsync<ENTITIES.UserAuthenticationGoogle> googleAuthRepository,
		UserPreferencesJsonSerializer preferencesSerializer,
		IAuditableRepositoryAsync<ENTITIES.User> userRepository)
	{
		_googleAuthRepository = googleAuthRepository;
		_preferencesSerializer = preferencesSerializer;
		_userRepository = userRepository;
	}

	#endregion

	#region <Methods>

	public async Task<ENTITIES.User> HandleAsync(SignInArgumentOf<GoogleSignInPayload> argument)
	{
		var existing = (await _googleAuthRepository.FindAsync(x => x.GoogleId == argument.Payload.GoogleId)).FirstOrDefault();

		if (existing is not null)
		{
			var user = await _userRepository.GetByIdAsync(existing.UserId);
			user!.LastLoginAt = DateTime.UtcNow;
			user.UpdatedByUserId = user.Id;
			user.UpdatedOnDate = DateTime.UtcNow;
			await _userRepository.SaveChangesAsync();
			return user;
		}

		return await CreateUserAsync(argument);
	}

	private async Task<ENTITIES.User> CreateUserAsync(SignInArgumentOf<GoogleSignInPayload> argument)
	{
		var now = DateTime.UtcNow;
		var defaultPreferences = _preferencesSerializer.Serialize(new UserPreferences());

		var user = new ENTITIES.User
		{
			CreateByUserId = 0,
			CreatedOnDateTime = now,
			DisplayName = argument.DisplayName,
			FirstName = argument.FirstName,
			InitialProvider = "Google",
			LastLoginAt = now,
			LastName = argument.LastName,
			PreferredProvider = "Google",
			UpdatedByUserId = 0,
			UpdatedOnDate = now,
			UserPreferences = defaultPreferences
		};

		await _userRepository.AddAsync(user);
		await _userRepository.SaveChangesAsync();

		var googleAuth = new ENTITIES.UserAuthenticationGoogle
		{
			AvatarUrl = argument.Payload.AvatarUrl,
			CreateByUserId = user.Id,
			CreatedOnDateTime = now,
			DisplayName = argument.DisplayName,
			Email = argument.Email,
			EmailVerified = argument.Payload.EmailVerified,
			FirstName = argument.FirstName,
			GoogleId = argument.Payload.GoogleId,
			LastName = argument.LastName,
			Locale = argument.Payload.Locale,
			UpdatedByUserId = user.Id,
			UpdatedOnDate = now,
			UserId = user.Id
		};

		await _googleAuthRepository.AddAsync(googleAuth);
		await _googleAuthRepository.SaveChangesAsync();

		return user;
	}

	#endregion
}
