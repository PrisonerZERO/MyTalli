namespace My.Talli.UnitTesting.Handlers.Authentication;

using Domain.Handlers.Authentication;
using Domain.Models;
using My.Talli.UnitTesting.Infrastructure.Builders;

using ENTITIES = Domain.Entities;

/// <summary>Tests</summary>
public class MicrosoftSignInHandlerTests
{
	#region <Methods>

	[Fact]
	public async Task ExistingEmailOnGoogle_LinksMicrosoftAuth()
	{
		var builder = new SignInHandlerBuilder();
		SeedGoogleUser(builder, "shared@example.com");

		var argument = CreateArgument("shared@example.com", "new-microsoft-id");
		var result = await builder.MicrosoftHandler.HandleAsync(argument);

		Assert.Equal(1, builder.UserRepository.Store.Count);
		Assert.Equal(1, builder.MicrosoftAuthRepository.Store.Count);
		Assert.Equal(result.Id, builder.MicrosoftAuthRepository.Store[0].Id);
		Assert.False(result.IsNewUser);
	}

	[Fact]
	public async Task ExistingMicrosoftUser_ReturnsExistingUser()
	{
		var builder = new SignInHandlerBuilder();
		SeedMicrosoftUser(builder);

		var argument = CreateArgument("existing@outlook.com", "microsoft-123");
		var result = await builder.MicrosoftHandler.HandleAsync(argument);

		Assert.Equal(1, result.Id);
		Assert.False(result.IsNewUser);
	}

	[Fact]
	public async Task ExistingMicrosoftUser_UpdatesLastLoginAt()
	{
		var builder = new SignInHandlerBuilder();
		SeedMicrosoftUser(builder);
		var originalLoginAt = builder.UserRepository.Store[0].LastLoginAt;

		var argument = CreateArgument("existing@outlook.com", "microsoft-123");
		var result = await builder.MicrosoftHandler.HandleAsync(argument);

		Assert.True(result.LastLoginAt > originalLoginAt);
	}

	[Fact]
	public async Task ExistingUser_WithNoRoles_SelfHealsUserRole()
	{
		var builder = new SignInHandlerBuilder();
		SeedMicrosoftUserWithoutRoles(builder);

		var argument = CreateArgument("existing@outlook.com", "microsoft-123");
		var result = await builder.MicrosoftHandler.HandleAsync(argument);

		Assert.Equal(1, builder.UserRoleRepository.Store.Count);
		Assert.Contains(Domain.Framework.Roles.User, result.Roles);
	}

	[Fact]
	public async Task NewUser_AssignsUserRole()
	{
		var builder = new SignInHandlerBuilder();

		var argument = CreateArgument("new@outlook.com", "new-microsoft-id");
		var result = await builder.MicrosoftHandler.HandleAsync(argument);

		Assert.Equal(1, builder.UserRoleRepository.Store.Count);
		Assert.Equal(Domain.Framework.Roles.User, builder.UserRoleRepository.Store[0].Role);
	}

	[Fact]
	public async Task NewUser_CreatesUserAndMicrosoftAuth()
	{
		var builder = new SignInHandlerBuilder();

		var argument = CreateArgument("new@outlook.com", "new-microsoft-id");
		var result = await builder.MicrosoftHandler.HandleAsync(argument);

		Assert.True(result.Id > 0);
		Assert.Equal(1, builder.UserRepository.Store.Count);
		Assert.Equal(1, builder.MicrosoftAuthRepository.Store.Count);
		Assert.Equal("new-microsoft-id", builder.MicrosoftAuthRepository.Store[0].MicrosoftId);
		Assert.Equal("new@outlook.com", builder.MicrosoftAuthRepository.Store[0].Email);
		Assert.Equal(result.Id, builder.MicrosoftAuthRepository.Store[0].Id);
	}

	[Fact]
	public async Task NewUser_IsNewUserTrue()
	{
		var builder = new SignInHandlerBuilder();

		var argument = CreateArgument("new@outlook.com", "new-microsoft-id");
		var result = await builder.MicrosoftHandler.HandleAsync(argument);

		Assert.True(result.IsNewUser);
	}

	[Fact]
	public async Task NewUser_SetsProviderToMicrosoft()
	{
		var builder = new SignInHandlerBuilder();

		var argument = CreateArgument("new@outlook.com", "new-microsoft-id");
		var result = await builder.MicrosoftHandler.HandleAsync(argument);

		Assert.Equal("Microsoft", result.InitialProvider);
		Assert.Equal("Microsoft", result.PreferredProvider);
	}

	private static SignInArgumentOf<MicrosoftSignInPayload> CreateArgument(string email, string microsoftId) => new()
	{
		DisplayName = "Test User",
		Email = email,
		FirstName = "Test",
		LastName = "User",
		Payload = new MicrosoftSignInPayload
		{
			MicrosoftId = microsoftId,
		}
	};

	private static void SeedGoogleUser(SignInHandlerBuilder builder, string email)
	{
		builder.UserRepository.InsertAsync(new ENTITIES.User
		{
			DisplayName = "Existing User", FirstName = "Existing", InitialProvider = "Google",
			LastLoginAt = DateTime.UtcNow.AddDays(-1), LastName = "User", PreferredProvider = "Google",
			UserPreferences = "{}",
		}).Wait();
		var userId = builder.UserRepository.Store[0].Id;
		builder.GoogleAuthRepository.InsertAsync(new ENTITIES.UserAuthenticationGoogle
		{
			AvatarUrl = "https://example.com/avatar.jpg", DisplayName = "Existing User",
			Email = email, EmailVerified = true, FirstName = "Existing",
			GoogleId = "google-123", Id = userId, LastName = "User", Locale = "en",
		}).Wait();
		builder.UserRoleRepository.InsertAsync(new ENTITIES.UserRole { Role = Domain.Framework.Roles.User, UserId = userId }).Wait();
		builder.CurrentUserService.Set(userId, "Existing User");
	}

	private static void SeedMicrosoftUser(SignInHandlerBuilder builder)
	{
		builder.UserRepository.InsertAsync(new ENTITIES.User
		{
			DisplayName = "Existing User", FirstName = "Existing", InitialProvider = "Microsoft",
			LastLoginAt = DateTime.UtcNow.AddDays(-1), LastName = "User", PreferredProvider = "Microsoft",
			UserPreferences = "{}",
		}).Wait();
		var userId = builder.UserRepository.Store[0].Id;
		builder.MicrosoftAuthRepository.InsertAsync(new ENTITIES.UserAuthenticationMicrosoft
		{
			DisplayName = "Existing User", Email = "existing@outlook.com", FirstName = "Existing",
			Id = userId, LastName = "User", MicrosoftId = "microsoft-123",
		}).Wait();
		builder.UserRoleRepository.InsertAsync(new ENTITIES.UserRole { Role = Domain.Framework.Roles.User, UserId = userId }).Wait();
		builder.CurrentUserService.Set(userId, "Existing User");
	}

	private static void SeedMicrosoftUserWithoutRoles(SignInHandlerBuilder builder)
	{
		builder.UserRepository.InsertAsync(new ENTITIES.User
		{
			DisplayName = "Existing User", FirstName = "Existing", InitialProvider = "Microsoft",
			LastLoginAt = DateTime.UtcNow.AddDays(-1), LastName = "User", PreferredProvider = "Microsoft",
			UserPreferences = "{}",
		}).Wait();
		var userId = builder.UserRepository.Store[0].Id;
		builder.MicrosoftAuthRepository.InsertAsync(new ENTITIES.UserAuthenticationMicrosoft
		{
			DisplayName = "Existing User", Email = "existing@outlook.com", FirstName = "Existing",
			Id = userId, LastName = "User", MicrosoftId = "microsoft-123",
		}).Wait();
		builder.CurrentUserService.Set(userId, "Existing User");
	}

	#endregion
}
