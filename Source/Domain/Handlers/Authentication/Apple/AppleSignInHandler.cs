namespace My.Talli.Domain.Handlers.Authentication;

using Domain.Components.JsonSerializers;
using Domain.Data.Interfaces;
using Domain.Models;

using ENTITIES = Domain.Entities;

/// <summary>Handler</summary>
public class AppleSignInHandler
{
	#region <Variables>

	private readonly IAuditableRepositoryAsync<ENTITIES.User> _userRepository;
	private readonly IAuditableRepositoryAsync<ENTITIES.UserAuthenticationApple> _appleAuthRepository;
	private readonly UserPreferencesJsonSerializer _preferencesSerializer;

	#endregion

	#region <Constructors>

	public AppleSignInHandler(
		IAuditableRepositoryAsync<ENTITIES.User> userRepository,
		IAuditableRepositoryAsync<ENTITIES.UserAuthenticationApple> appleAuthRepository,
		UserPreferencesJsonSerializer preferencesSerializer)
	{
		_appleAuthRepository = appleAuthRepository;
		_preferencesSerializer = preferencesSerializer;
		_userRepository = userRepository;
	}

	#endregion

	#region <Methods>

	public async Task<ENTITIES.User> HandleAsync(SignInArgumentOf<AppleSignInPayload> argument)
	{
		var existing = (await _appleAuthRepository.FindAsync(x => x.AppleId == argument.Payload.AppleId)).FirstOrDefault();

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

	private async Task<ENTITIES.User> CreateUserAsync(SignInArgumentOf<AppleSignInPayload> argument)
	{
		var now = DateTime.UtcNow;
		var defaultPreferences = _preferencesSerializer.Serialize(new UserPreferences());

		var user = new ENTITIES.User
		{
			CreateByUserId = 0,
			CreatedOnDateTime = now,
			DisplayName = argument.DisplayName,
			FirstName = argument.FirstName,
			InitialProvider = "Apple",
			LastLoginAt = now,
			LastName = argument.LastName,
			PreferredProvider = "Apple",
			UserPreferences = defaultPreferences
		};

		await _userRepository.AddAsync(user);
		await _userRepository.SaveChangesAsync();

		var appleAuth = new ENTITIES.UserAuthenticationApple
		{
			AppleId = argument.Payload.AppleId,
			CreateByUserId = user.Id,
			CreatedOnDateTime = now,
			DisplayName = argument.DisplayName,
			Email = argument.Email,
			FirstName = argument.FirstName,
			IsPrivateRelay = argument.Payload.IsPrivateRelay,
			LastName = argument.LastName,
			UserId = user.Id
		};

		await _appleAuthRepository.AddAsync(appleAuth);
		await _appleAuthRepository.SaveChangesAsync();

		return user;
	}

	#endregion
}
