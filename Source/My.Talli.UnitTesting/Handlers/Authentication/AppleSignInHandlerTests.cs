namespace My.Talli.UnitTesting.Handlers.Authentication;

using Domain.Handlers.Authentication;
using Domain.Models;
using My.Talli.UnitTesting.Infrastructure.Builders;

using ENTITIES = Domain.Entities;

/// <summary>Tests</summary>
public class AppleSignInHandlerTests
{
	#region <Methods>

	[Fact]
	public async Task ExistingAppleUser_ReturnsExistingUser()
	{
		var builder = new SignInHandlerBuilder();
		SeedAppleUser(builder);

		var argument = CreateArgument("existing@icloud.com", "apple-123");
		var result = await builder.AppleHandler.HandleAsync(argument);

		Assert.Equal(1, result.Id);
		Assert.False(result.IsNewUser);
	}

	[Fact]
	public async Task ExistingAppleUser_UpdatesLastLoginAt()
	{
		var builder = new SignInHandlerBuilder();
		SeedAppleUser(builder);
		var originalLoginAt = builder.UserRepository.Store[0].LastLoginAt;

		var argument = CreateArgument("existing@icloud.com", "apple-123");
		var result = await builder.AppleHandler.HandleAsync(argument);

		Assert.True(result.LastLoginAt > originalLoginAt);
	}

	[Fact]
	public async Task ExistingEmailOnGoogle_LinksAppleAuth()
	{
		var builder = new SignInHandlerBuilder();
		SeedGoogleUser(builder, "shared@example.com");

		var argument = CreateArgument("shared@example.com", "new-apple-id");
		var result = await builder.AppleHandler.HandleAsync(argument);

		Assert.Equal(1, builder.UserRepository.Store.Count);
		Assert.Equal(1, builder.AppleAuthRepository.Store.Count);
		Assert.Equal(result.Id, builder.AppleAuthRepository.Store[0].Id);
		Assert.False(result.IsNewUser);
	}

	[Fact]
	public async Task ExistingUser_WithNoRoles_SelfHealsUserRole()
	{
		var builder = new SignInHandlerBuilder();
		SeedAppleUserWithoutRoles(builder);

		var argument = CreateArgument("existing@icloud.com", "apple-123");
		var result = await builder.AppleHandler.HandleAsync(argument);

		Assert.Equal(1, builder.UserRoleRepository.Store.Count);
		Assert.Contains(Domain.Framework.Roles.User, result.Roles);
	}

	[Fact]
	public async Task NewUser_AssignsUserRole()
	{
		var builder = new SignInHandlerBuilder();

		var argument = CreateArgument("new@icloud.com", "new-apple-id");
		var result = await builder.AppleHandler.HandleAsync(argument);

		Assert.Equal(1, builder.UserRoleRepository.Store.Count);
		Assert.Equal(Domain.Framework.Roles.User, builder.UserRoleRepository.Store[0].Role);
	}

	[Fact]
	public async Task NewUser_CreatesUserAndAppleAuth()
	{
		var builder = new SignInHandlerBuilder();

		var argument = CreateArgument("new@icloud.com", "new-apple-id");
		var result = await builder.AppleHandler.HandleAsync(argument);

		Assert.True(result.Id > 0);
		Assert.Equal(1, builder.UserRepository.Store.Count);
		Assert.Equal(1, builder.AppleAuthRepository.Store.Count);
		Assert.Equal("new-apple-id", builder.AppleAuthRepository.Store[0].AppleId);
		Assert.Equal("new@icloud.com", builder.AppleAuthRepository.Store[0].Email);
		Assert.Equal(result.Id, builder.AppleAuthRepository.Store[0].Id);
	}

	[Fact]
	public async Task NewUser_IsNewUserTrue()
	{
		var builder = new SignInHandlerBuilder();

		var argument = CreateArgument("new@icloud.com", "new-apple-id");
		var result = await builder.AppleHandler.HandleAsync(argument);

		Assert.True(result.IsNewUser);
	}

	[Fact]
	public async Task NewUser_SetsProviderToApple()
	{
		var builder = new SignInHandlerBuilder();

		var argument = CreateArgument("new@icloud.com", "new-apple-id");
		var result = await builder.AppleHandler.HandleAsync(argument);

		Assert.Equal("Apple", result.InitialProvider);
		Assert.Equal("Apple", result.PreferredProvider);
	}

	private static SignInArgumentOf<AppleSignInPayload> CreateArgument(string email, string appleId) => new()
	{
		DisplayName = "Test User",
		Email = email,
		FirstName = "Test",
		LastName = "User",
		Payload = new AppleSignInPayload
		{
			AppleId = appleId,
			IsPrivateRelay = false,
		}
	};

	private static void SeedAppleUser(SignInHandlerBuilder builder)
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
			AppleId = "apple-123", DisplayName = "Existing User", Email = "existing@icloud.com",
			FirstName = "Existing", Id = userId, LastName = "User",
		}).Wait();
		builder.UserRoleRepository.InsertAsync(new ENTITIES.UserRole { Role = Domain.Framework.Roles.User, UserId = userId }).Wait();
		builder.CurrentUserService.Set(userId, "Existing User");
	}

	private static void SeedAppleUserWithoutRoles(SignInHandlerBuilder builder)
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
			AppleId = "apple-123", DisplayName = "Existing User", Email = "existing@icloud.com",
			FirstName = "Existing", Id = userId, LastName = "User",
		}).Wait();
		builder.CurrentUserService.Set(userId, "Existing User");
	}

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

	#endregion
}
