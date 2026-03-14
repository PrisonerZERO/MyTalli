namespace My.Talli.Domain.Handlers.Authentication;

using Domain.Components.JsonSerializers;
using Domain.Data.Interfaces;
using Domain.Models;

using ENTITIES = Domain.Entities;

/// <summary>Handler</summary>
public class MicrosoftSignInHandler
{
	#region <Variables>

	private readonly IAuditableRepositoryAsync<ENTITIES.User> _userRepository;
	private readonly IAuditableRepositoryAsync<ENTITIES.UserAuthenticationMicrosoft> _microsoftAuthRepository;
	private readonly UserPreferencesJsonSerializer _preferencesSerializer;

	#endregion

	#region <Constructors>

	public MicrosoftSignInHandler(
		IAuditableRepositoryAsync<ENTITIES.User> userRepository,
		IAuditableRepositoryAsync<ENTITIES.UserAuthenticationMicrosoft> microsoftAuthRepository,
		UserPreferencesJsonSerializer preferencesSerializer)
	{
		_microsoftAuthRepository = microsoftAuthRepository;
		_preferencesSerializer = preferencesSerializer;
		_userRepository = userRepository;
	}

	#endregion

	#region <Methods>

	public async Task<ENTITIES.User> HandleAsync(SignInArgumentOf<MicrosoftSignInPayload> argument)
	{
		var existing = (await _microsoftAuthRepository.FindAsync(x => x.MicrosoftId == argument.Payload.MicrosoftId)).FirstOrDefault();

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

	private async Task<ENTITIES.User> CreateUserAsync(SignInArgumentOf<MicrosoftSignInPayload> argument)
	{
		var now = DateTime.UtcNow;
		var defaultPreferences = _preferencesSerializer.Serialize(new UserPreferences());

		var user = new ENTITIES.User
		{
			CreateByUserId = 0,
			CreatedOnDateTime = now,
			DisplayName = argument.DisplayName,
			FirstName = argument.FirstName,
			InitialProvider = "Microsoft",
			LastLoginAt = now,
			LastName = argument.LastName,
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
			DisplayName = argument.DisplayName,
			Email = argument.Email,
			FirstName = argument.FirstName,
			LastName = argument.LastName,
			MicrosoftId = argument.Payload.MicrosoftId,
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
