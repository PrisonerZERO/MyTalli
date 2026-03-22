namespace My.Talli.UnitTesting.Handlers.Authentication;

using Domain.Handlers.Authentication;
using Domain.Models;
using My.Talli.UnitTesting.Infrastructure.Builders;

using ENTITIES = Domain.Entities;

/// <summary>Tests</summary>
public class GoogleSignInHandlerTests
{
	#region <Methods>

	[Fact]
	public async Task ExistingEmailOnApple_LinksGoogleAuth()
	{
		var builder = new SignInHandlerBuilder();
		SeedAppleUser(builder, "shared@example.com");

		var argument = CreateArgument("shared@example.com", "new-google-id");
		var result = await builder.GoogleHandler.HandleAsync(argument);

		Assert.Equal(1, builder.UserRepository.Store.Count);
		Assert.Equal(1, builder.GoogleAuthRepository.Store.Count);
		Assert.Equal(result.Id, builder.GoogleAuthRepository.Store[0].Id);
		Assert.False(result.IsNewUser);
	}

	[Fact]
	public async Task ExistingEmailOnMicrosoft_LinksGoogleAuth_NoNewUser()
	{
		var builder = new SignInHandlerBuilder();
		SeedMicrosoftUser(builder, "shared@example.com");

		var argument = CreateArgument("shared@example.com", "new-google-id");
		var result = await builder.GoogleHandler.HandleAsync(argument);

		Assert.Equal(1, builder.UserRepository.Store.Count);
		Assert.Equal(1, builder.GoogleAuthRepository.Store.Count);
		Assert.Equal(result.Id, builder.GoogleAuthRepository.Store[0].Id);
		Assert.False(result.IsNewUser);
	}

	[Fact]
	public async Task ExistingGoogleUser_DoesNotCreateDuplicateUser()
	{
		var builder = new SignInHandlerBuilder();
		SeedGoogleUser(builder);

		var argument = CreateArgument("existing@gmail.com", "google-123");
		await builder.GoogleHandler.HandleAsync(argument);

		Assert.Equal(1, builder.UserRepository.Store.Count);
	}

	[Fact]
	public async Task ExistingGoogleUser_IsNewUserFalse()
	{
		var builder = new SignInHandlerBuilder();
		SeedGoogleUser(builder);

		var argument = CreateArgument("existing@gmail.com", "google-123");
		var result = await builder.GoogleHandler.HandleAsync(argument);

		Assert.False(result.IsNewUser);
	}

	[Fact]
	public async Task ExistingGoogleUser_ResolvesRoles()
	{
		var builder = new SignInHandlerBuilder();
		SeedGoogleUser(builder);

		var argument = CreateArgument("existing@gmail.com", "google-123");
		var result = await builder.GoogleHandler.HandleAsync(argument);

		Assert.Contains(Domain.Framework.Roles.User, result.Roles);
	}

	[Fact]
	public async Task ExistingGoogleUser_ReturnsExistingUserId()
	{
		var builder = new SignInHandlerBuilder();
		SeedGoogleUser(builder);

		var argument = CreateArgument("existing@gmail.com", "google-123");
		var result = await builder.GoogleHandler.HandleAsync(argument);

		Assert.Equal(1, result.Id);
	}

	[Fact]
	public async Task ExistingGoogleUser_UpdatesLastLoginAt()
	{
		var builder = new SignInHandlerBuilder();
		SeedGoogleUser(builder);
		var originalLoginAt = builder.UserRepository.Store[0].LastLoginAt;

		var argument = CreateArgument("existing@gmail.com", "google-123");
		var result = await builder.GoogleHandler.HandleAsync(argument);

		Assert.True(result.LastLoginAt > originalLoginAt);
	}

	[Fact]
	public async Task ExistingUser_WithNoRoles_SelfHealsUserRole()
	{
		var builder = new SignInHandlerBuilder();
		SeedGoogleUserWithoutRoles(builder);

		var argument = CreateArgument("existing@gmail.com", "google-123");
		var result = await builder.GoogleHandler.HandleAsync(argument);

		Assert.Equal(1, builder.UserRoleRepository.Store.Count);
		Assert.Contains(Domain.Framework.Roles.User, result.Roles);
	}

	[Fact]
	public async Task NewUser_AssignsUserRole()
	{
		var builder = new SignInHandlerBuilder();

		var argument = CreateArgument("new@gmail.com", "new-google-id");
		var result = await builder.GoogleHandler.HandleAsync(argument);

		Assert.Equal(1, builder.UserRoleRepository.Store.Count);
		Assert.Equal(Domain.Framework.Roles.User, builder.UserRoleRepository.Store[0].Role);
		Assert.Equal(result.Id, builder.UserRoleRepository.Store[0].UserId);
	}

	[Fact]
	public async Task NewUser_CreatesUserAndGoogleAuth()
	{
		var builder = new SignInHandlerBuilder();

		var argument = CreateArgument("new@gmail.com", "new-google-id");
		var result = await builder.GoogleHandler.HandleAsync(argument);

		Assert.True(result.Id > 0);
		Assert.Equal(1, builder.UserRepository.Store.Count);
		Assert.Equal(1, builder.GoogleAuthRepository.Store.Count);
		Assert.Equal("new-google-id", builder.GoogleAuthRepository.Store[0].GoogleId);
		Assert.Equal("new@gmail.com", builder.GoogleAuthRepository.Store[0].Email);
		Assert.Equal(result.Id, builder.GoogleAuthRepository.Store[0].Id);
	}

	[Fact]
	public async Task NewUser_IsNewUserTrue()
	{
		var builder = new SignInHandlerBuilder();

		var argument = CreateArgument("new@gmail.com", "new-google-id");
		var result = await builder.GoogleHandler.HandleAsync(argument);

		Assert.True(result.IsNewUser);
	}

	[Fact]
	public async Task NewUser_SetsCurrentUserServiceAfterInsert()
	{
		var builder = new SignInHandlerBuilder();

		var argument = CreateArgument("new@gmail.com", "new-google-id");
		var result = await builder.GoogleHandler.HandleAsync(argument);

		Assert.Equal(result.Id, builder.CurrentUserService.UserId);
		Assert.Equal(argument.DisplayName, builder.CurrentUserService.DisplayName);
	}

	[Fact]
	public async Task NewUser_SetsDefaultPreferences()
	{
		var builder = new SignInHandlerBuilder();

		var argument = CreateArgument("new@gmail.com", "new-google-id");
		var result = await builder.GoogleHandler.HandleAsync(argument);

		Assert.Contains("emailPreferences", result.UserPreferences);
		Assert.Contains("funGreetings", result.UserPreferences);
	}

	[Fact]
	public async Task NewUser_SetsDisplayNameAndNames()
	{
		var builder = new SignInHandlerBuilder();

		var argument = CreateArgument("new@gmail.com", "new-google-id");
		var result = await builder.GoogleHandler.HandleAsync(argument);

		Assert.Equal("Test User", result.DisplayName);
		Assert.Equal("Test", result.FirstName);
		Assert.Equal("User", result.LastName);
	}

	[Fact]
	public async Task NewUser_SetsProviderToGoogle()
	{
		var builder = new SignInHandlerBuilder();

		var argument = CreateArgument("new@gmail.com", "new-google-id");
		var result = await builder.GoogleHandler.HandleAsync(argument);

		Assert.Equal("Google", result.InitialProvider);
		Assert.Equal("Google", result.PreferredProvider);
	}

	[Fact]
	public async Task NewUser_SetsRolesOnReturnedUser()
	{
		var builder = new SignInHandlerBuilder();

		var argument = CreateArgument("new@gmail.com", "new-google-id");
		var result = await builder.GoogleHandler.HandleAsync(argument);

		Assert.Single(result.Roles);
		Assert.Contains(Domain.Framework.Roles.User, result.Roles);
	}

	private static SignInArgumentOf<GoogleSignInPayload> CreateArgument(string email, string googleId) => new()
	{
		DisplayName = "Test User",
		Email = email,
		FirstName = "Test",
		LastName = "User",
		Payload = new GoogleSignInPayload
		{
			AvatarUrl = "https://example.com/avatar.jpg",
			EmailVerified = true,
			GoogleId = googleId,
			Locale = "en",
		}
	};

	private static void SeedAppleUser(SignInHandlerBuilder builder, string email)
	{
		builder.UserRepository.InsertAsync(new ENTITIES.User
		{
			DisplayName = "Existing User", FirstName = "Existing", InitialProvider = "Apple",
			LastLoginAt = DateTime.UtcNow.AddDays(-1), LastName = "User", PreferredProvider = "Apple",
			UserPreferences = "{}",
		}).Wait();
		var userId = builder.UserRepository.Store[0].Id;
		builder.AppleAuthRepository.InsertAsync(new ENTITIES.UserAuthenticationApple
		{
			AppleId = "apple-456", DisplayName = "Existing User", Email = email,
			FirstName = "Existing", Id = userId, LastName = "User",
		}).Wait();
		builder.UserRoleRepository.InsertAsync(new ENTITIES.UserRole { Role = Domain.Framework.Roles.User, UserId = userId }).Wait();
		builder.CurrentUserService.Set(userId, "Existing User");
	}

	private static void SeedGoogleUser(SignInHandlerBuilder builder)
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
			Email = "existing@gmail.com", EmailVerified = true, FirstName = "Existing",
			GoogleId = "google-123", Id = userId, LastName = "User", Locale = "en",
		}).Wait();
		builder.UserRoleRepository.InsertAsync(new ENTITIES.UserRole { Role = Domain.Framework.Roles.User, UserId = userId }).Wait();
		builder.CurrentUserService.Set(userId, "Existing User");
	}

	private static void SeedGoogleUserWithoutRoles(SignInHandlerBuilder builder)
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
			Email = "existing@gmail.com", EmailVerified = true, FirstName = "Existing",
			GoogleId = "google-123", Id = userId, LastName = "User", Locale = "en",
		}).Wait();
		builder.CurrentUserService.Set(userId, "Existing User");
	}

	private static void SeedMicrosoftUser(SignInHandlerBuilder builder, string email)
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
			DisplayName = "Existing User", Email = email, FirstName = "Existing",
			Id = userId, LastName = "User", MicrosoftId = "microsoft-789",
		}).Wait();
		builder.UserRoleRepository.InsertAsync(new ENTITIES.UserRole { Role = Domain.Framework.Roles.User, UserId = userId }).Wait();
		builder.CurrentUserService.Set(userId, "Existing User");
	}

	#endregion
}
