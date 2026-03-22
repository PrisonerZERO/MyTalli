namespace My.Talli.UnitTesting.Handlers.Authentication;

using Domain.Handlers.Authentication;
using Domain.Models;
using My.Talli.UnitTesting.Infrastructure.Builders;

/// <summary>Tests</summary>
public class GoogleSignInHandlerTests
{
	#region <Methods>

	[Fact]
	public async Task ExistingEmailOnApple_LinksGoogleAuth()
	{
		var builder = new SignInHandlerBuilder();
		var seededUser = await SeedAppleUserAsync(builder, "shared@example.com");

		var argument = CreateArgument("shared@example.com", "new-google-id");
		var result = await builder.GoogleHandler.HandleAsync(argument);

		var googleAuths = await builder.GoogleAuthAdapter.GetAllAsync();
		Assert.Equal(seededUser.Id, result.Id);
		Assert.Single(googleAuths);
		Assert.False(result.IsNewUser);
	}

	[Fact]
	public async Task ExistingEmailOnMicrosoft_LinksGoogleAuth_NoNewUser()
	{
		var builder = new SignInHandlerBuilder();
		var seededUser = await SeedMicrosoftUserAsync(builder, "shared@example.com");

		var argument = CreateArgument("shared@example.com", "new-google-id");
		var result = await builder.GoogleHandler.HandleAsync(argument);

		var googleAuths = await builder.GoogleAuthAdapter.GetAllAsync();
		Assert.Equal(seededUser.Id, result.Id);
		Assert.Single(googleAuths);
		Assert.False(result.IsNewUser);
	}

	[Fact]
	public async Task ExistingGoogleUser_DoesNotCreateDuplicateUser()
	{
		var builder = new SignInHandlerBuilder();
		await SeedGoogleUserAsync(builder);

		var argument = CreateArgument("existing@gmail.com", "google-123");
		await builder.GoogleHandler.HandleAsync(argument);

		var users = await builder.UserAdapter.GetAllAsync();
		Assert.Single(users);
	}

	[Fact]
	public async Task ExistingGoogleUser_IsNewUserFalse()
	{
		var builder = new SignInHandlerBuilder();
		await SeedGoogleUserAsync(builder);

		var argument = CreateArgument("existing@gmail.com", "google-123");
		var result = await builder.GoogleHandler.HandleAsync(argument);

		Assert.False(result.IsNewUser);
	}

	[Fact]
	public async Task ExistingGoogleUser_ResolvesRoles()
	{
		var builder = new SignInHandlerBuilder();
		await SeedGoogleUserAsync(builder);

		var argument = CreateArgument("existing@gmail.com", "google-123");
		var result = await builder.GoogleHandler.HandleAsync(argument);

		Assert.Contains(Domain.Framework.Roles.User, result.Roles);
	}

	[Fact]
	public async Task ExistingGoogleUser_ReturnsExistingUserId()
	{
		var builder = new SignInHandlerBuilder();
		var seededUser = await SeedGoogleUserAsync(builder);

		var argument = CreateArgument("existing@gmail.com", "google-123");
		var result = await builder.GoogleHandler.HandleAsync(argument);

		Assert.Equal(seededUser.Id, result.Id);
	}

	[Fact]
	public async Task ExistingGoogleUser_UpdatesLastLoginAt()
	{
		var builder = new SignInHandlerBuilder();
		var seededUser = await SeedGoogleUserAsync(builder);

		var argument = CreateArgument("existing@gmail.com", "google-123");
		var result = await builder.GoogleHandler.HandleAsync(argument);

		Assert.True(result.LastLoginAt > seededUser.LastLoginAt);
	}

	[Fact]
	public async Task ExistingUser_WithNoRoles_SelfHealsUserRole()
	{
		var builder = new SignInHandlerBuilder();
		await SeedGoogleUserWithoutRolesAsync(builder);

		var argument = CreateArgument("existing@gmail.com", "google-123");
		var result = await builder.GoogleHandler.HandleAsync(argument);

		var roles = await builder.UserRoleAdapter.GetAllAsync();
		Assert.Single(roles);
		Assert.Contains(Domain.Framework.Roles.User, result.Roles);
	}

	[Fact]
	public async Task NewUser_AssignsUserRole()
	{
		var builder = new SignInHandlerBuilder();

		var argument = CreateArgument("new@gmail.com", "new-google-id");
		var result = await builder.GoogleHandler.HandleAsync(argument);

		var roles = await builder.UserRoleAdapter.GetAllAsync();
		var role = Assert.Single(roles);
		Assert.Equal(Domain.Framework.Roles.User, role.Role);
		Assert.Equal(result.Id, role.UserId);
	}

	[Fact]
	public async Task NewUser_CreatesUserAndGoogleAuth()
	{
		var builder = new SignInHandlerBuilder();

		var argument = CreateArgument("new@gmail.com", "new-google-id");
		var result = await builder.GoogleHandler.HandleAsync(argument);

		Assert.True(result.Id > 0);

		var users = await builder.UserAdapter.GetAllAsync();
		Assert.Single(users);

		var googleAuths = await builder.GoogleAuthAdapter.GetAllAsync();
		var auth = Assert.Single(googleAuths);
		Assert.Equal("new-google-id", auth.GoogleId);
		Assert.Equal("new@gmail.com", auth.Email);
		Assert.Equal(result.Id, auth.Id);
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

	private static async Task<User> SeedAppleUserAsync(SignInHandlerBuilder builder, string email)
	{
		var argument = new SignInArgumentOf<AppleSignInPayload>
		{
			DisplayName = "Existing User", Email = email, FirstName = "Existing", LastName = "User",
			Payload = new AppleSignInPayload { AppleId = "apple-456", IsPrivateRelay = false }
		};
		return await builder.AppleHandler.HandleAsync(argument);
	}

	private static async Task<User> SeedGoogleUserAsync(SignInHandlerBuilder builder)
	{
		var argument = new SignInArgumentOf<GoogleSignInPayload>
		{
			DisplayName = "Existing User", Email = "existing@gmail.com", FirstName = "Existing", LastName = "User",
			Payload = new GoogleSignInPayload { AvatarUrl = "https://example.com/avatar.jpg", EmailVerified = true, GoogleId = "google-123", Locale = "en" }
		};
		return await builder.GoogleHandler.HandleAsync(argument);
	}

	private static async Task<User> SeedGoogleUserWithoutRolesAsync(SignInHandlerBuilder builder)
	{
		var user = await SeedGoogleUserAsync(builder);

		var roles = (await builder.UserRoleAdapter.FindAsync(x => x.UserId == user.Id)).ToList();
		foreach (var role in roles)
			await builder.UserRoleAdapter.DeleteAsync(role);

		return user;
	}

	private static async Task<User> SeedMicrosoftUserAsync(SignInHandlerBuilder builder, string email)
	{
		var argument = new SignInArgumentOf<MicrosoftSignInPayload>
		{
			DisplayName = "Existing User", Email = email, FirstName = "Existing", LastName = "User",
			Payload = new MicrosoftSignInPayload { MicrosoftId = "microsoft-789" }
		};
		return await builder.MicrosoftHandler.HandleAsync(argument);
	}

	#endregion
}
