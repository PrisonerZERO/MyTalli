namespace My.Talli.Domain.Handlers.Authentication;

using Domain.Components.JsonSerializers;
using Domain.Data.Interfaces;
using Domain.Models;

using ENTITIES = Domain.Entities.Entities;

/// <summary>Handler</summary>
public class MicrosoftSignInHandler
{
	#region <Variables>

	private readonly IAuditableRepositoryAsync<ENTITIES.UserAuthenticationMicrosoft> _microsoftAuthRepository;
	private readonly UserPreferencesJsonSerializer _preferencesSerializer;
	private readonly IAuditableRepositoryAsync<ENTITIES.User> _userRepository;

	#endregion

	#region <Constructors>

	public MicrosoftSignInHandler(
		IAuditableRepositoryAsync<ENTITIES.UserAuthenticationMicrosoft> microsoftAuthRepository,
		UserPreferencesJsonSerializer preferencesSerializer,
		IAuditableRepositoryAsync<ENTITIES.User> userRepository)
	{
		_microsoftAuthRepository = microsoftAuthRepository;
		_preferencesSerializer = preferencesSerializer;
		_userRepository = userRepository;
	}

	#endregion

	#region <Methods>

	public async Task<ENTITIES.User> HandleAsync(
		string microsoftId,
		string email,
		string displayName,
		string firstName,
		string lastName)
	{
		var existing = (await _microsoftAuthRepository.FindAsync(x => x.MicrosoftId == microsoftId)).FirstOrDefault();

		if (existing is not null)
		{
			var user = await _userRepository.GetByIdAsync(existing.UserId);
			user!.LastLoginAt = DateTime.UtcNow;
			user.UpdatedByUserId = user.Id;
			user.UpdatedOnDate = DateTime.UtcNow;
			await _userRepository.SaveChangesAsync();
			return user;
		}

		return await CreateUserAsync(microsoftId, email, displayName, firstName, lastName);
	}

	private async Task<ENTITIES.User> CreateUserAsync(
		string microsoftId,
		string email,
		string displayName,
		string firstName,
		string lastName)
	{
		var now = DateTime.UtcNow;
		var defaultPreferences = _preferencesSerializer.Serialize(new UserPreferences());

		var user = new ENTITIES.User
		{
			CreateByUserId = 0,
			CreatedOnDateTime = now,
			DisplayName = displayName,
			FirstName = firstName,
			InitialProvider = "Microsoft",
			LastLoginAt = now,
			LastName = lastName,
			PreferredProvider = "Microsoft",
			UpdatedByUserId = 0,
			UpdatedOnDate = now,
			UserPreferences = defaultPreferences
		};

		await _userRepository.AddAsync(user);
		await _userRepository.SaveChangesAsync();

		var microsoftAuth = new ENTITIES.UserAuthenticationMicrosoft
		{
			CreateByUserId = user.Id,
			CreatedOnDateTime = now,
			DisplayName = displayName,
			Email = email,
			FirstName = firstName,
			LastName = lastName,
			MicrosoftId = microsoftId,
			UpdatedByUserId = user.Id,
			UpdatedOnDate = now,
			UserId = user.Id
		};

		await _microsoftAuthRepository.AddAsync(microsoftAuth);
		await _microsoftAuthRepository.SaveChangesAsync();

		return user;
	}

	#endregion
}
