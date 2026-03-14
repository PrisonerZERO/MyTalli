namespace My.Talli.Domain.Handlers.Authentication;

using Domain.Components.JsonSerializers;
using Domain.Data.Interfaces;
using Domain.Models;

using ENTITIES = Domain.Entities.Entities;

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

	public async Task<ENTITIES.User> HandleAsync(
		string googleId,
		string email,
		string displayName,
		string firstName,
		string lastName,
		string avatarUrl,
		bool emailVerified,
		string locale)
	{
		var existing = (await _googleAuthRepository.FindAsync(x => x.GoogleId == googleId)).FirstOrDefault();

		if (existing is not null)
		{
			var user = await _userRepository.GetByIdAsync(existing.UserId);
			user!.LastLoginAt = DateTime.UtcNow;
			user.UpdatedByUserId = user.Id;
			user.UpdatedOnDate = DateTime.UtcNow;
			await _userRepository.SaveChangesAsync();
			return user;
		}

		return await CreateUserAsync(googleId, email, displayName, firstName, lastName, avatarUrl, emailVerified, locale);
	}

	private async Task<ENTITIES.User> CreateUserAsync(
		string googleId,
		string email,
		string displayName,
		string firstName,
		string lastName,
		string avatarUrl,
		bool emailVerified,
		string locale)
	{
		var now = DateTime.UtcNow;
		var defaultPreferences = _preferencesSerializer.Serialize(new UserPreferences());

		var user = new ENTITIES.User
		{
			CreateByUserId = 0,
			CreatedOnDateTime = now,
			DisplayName = displayName,
			FirstName = firstName,
			InitialProvider = "Google",
			LastLoginAt = now,
			LastName = lastName,
			PreferredProvider = "Google",
			UpdatedByUserId = 0,
			UpdatedOnDate = now,
			UserPreferences = defaultPreferences
		};

		await _userRepository.AddAsync(user);
		await _userRepository.SaveChangesAsync();

		var googleAuth = new ENTITIES.UserAuthenticationGoogle
		{
			AvatarUrl = avatarUrl,
			CreateByUserId = user.Id,
			CreatedOnDateTime = now,
			DisplayName = displayName,
			Email = email,
			EmailVerified = emailVerified,
			FirstName = firstName,
			GoogleId = googleId,
			LastName = lastName,
			Locale = locale,
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
