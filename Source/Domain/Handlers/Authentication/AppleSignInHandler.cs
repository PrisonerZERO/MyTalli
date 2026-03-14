namespace My.Talli.Domain.Handlers.Authentication;

using Domain.Components.JsonSerializers;
using Domain.Data.Interfaces;
using Domain.Models;

using ENTITIES = Domain.Entities.Entities;

/// <summary>Handler</summary>
public class AppleSignInHandler
{
	#region <Variables>

	private readonly IAuditableRepositoryAsync<ENTITIES.UserAuthenticationApple> _appleAuthRepository;
	private readonly UserPreferencesJsonSerializer _preferencesSerializer;
	private readonly IAuditableRepositoryAsync<ENTITIES.User> _userRepository;

	#endregion

	#region <Constructors>

	public AppleSignInHandler(
		IAuditableRepositoryAsync<ENTITIES.UserAuthenticationApple> appleAuthRepository,
		UserPreferencesJsonSerializer preferencesSerializer,
		IAuditableRepositoryAsync<ENTITIES.User> userRepository)
	{
		_appleAuthRepository = appleAuthRepository;
		_preferencesSerializer = preferencesSerializer;
		_userRepository = userRepository;
	}

	#endregion

	#region <Methods>

	public async Task<ENTITIES.User> HandleAsync(
		string appleId,
		string email,
		string displayName,
		string firstName,
		string lastName,
		bool isPrivateRelay)
	{
		var existing = (await _appleAuthRepository.FindAsync(x => x.AppleId == appleId)).FirstOrDefault();

		if (existing is not null)
		{
			var user = await _userRepository.GetByIdAsync(existing.UserId);
			user!.LastLoginAt = DateTime.UtcNow;
			user.UpdatedByUserId = user.Id;
			user.UpdatedOnDate = DateTime.UtcNow;
			await _userRepository.SaveChangesAsync();
			return user;
		}

		return await CreateUserAsync(appleId, email, displayName, firstName, lastName, isPrivateRelay);
	}

	private async Task<ENTITIES.User> CreateUserAsync(
		string appleId,
		string email,
		string displayName,
		string firstName,
		string lastName,
		bool isPrivateRelay)
	{
		var now = DateTime.UtcNow;
		var defaultPreferences = _preferencesSerializer.Serialize(new UserPreferences());

		var user = new ENTITIES.User
		{
			CreateByUserId = 0,
			CreatedOnDateTime = now,
			DisplayName = displayName,
			FirstName = firstName,
			InitialProvider = "Apple",
			LastLoginAt = now,
			LastName = lastName,
			PreferredProvider = "Apple",
			UpdatedByUserId = 0,
			UpdatedOnDate = now,
			UserPreferences = defaultPreferences
		};

		await _userRepository.AddAsync(user);
		await _userRepository.SaveChangesAsync();

		var appleAuth = new ENTITIES.UserAuthenticationApple
		{
			AppleId = appleId,
			CreateByUserId = user.Id,
			CreatedOnDateTime = now,
			DisplayName = displayName,
			Email = email,
			FirstName = firstName,
			IsPrivateRelay = isPrivateRelay,
			LastName = lastName,
			UpdatedByUserId = user.Id,
			UpdatedOnDate = now,
			UserId = user.Id
		};

		await _appleAuthRepository.AddAsync(appleAuth);
		await _appleAuthRepository.SaveChangesAsync();

		return user;
	}

	#endregion
}
