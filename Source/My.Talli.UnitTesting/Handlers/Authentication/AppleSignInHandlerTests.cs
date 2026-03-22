namespace My.Talli.UnitTesting.Handlers.Authentication;

using Domain.Handlers.Authentication;
using Domain.Models;
using My.Talli.UnitTesting.Infrastructure.Builders;

/// <summary>Tests</summary>
public class AppleSignInHandlerTests
{
	#region <Methods>

	[Fact]
	public async Task ExistingAppleUser_ReturnsExistingUser()
	{
		var builder = new SignInHandlerBuilder();
		var seededUser = await SeedAppleUserAsync(builder);

		var argument = CreateArgument("existing@icloud.com", "apple-123");
		var result = await builder.AppleHandler.HandleAsync(argument);

		Assert.Equal(seededUser.Id, result.Id);
		Assert.False(result.IsNewUser);
	}

	[Fact]
	public async Task ExistingAppleUser_UpdatesLastLoginAt()
	{
		var builder = new SignInHandlerBuilder();
		var seededUser = await SeedAppleUserAsync(builder);

		var argument = CreateArgument("existing@icloud.com", "apple-123");
		var result = await builder.AppleHandler.HandleAsync(argument);

		Assert.True(result.LastLoginAt > seededUser.LastLoginAt);
	}

	[Fact]
	public async Task ExistingEmailOnGoogle_LinksAppleAuth()
	{
		var builder = new SignInHandlerBuilder();
		var seededUser = await SeedGoogleUserAsync(builder, "shared@example.com");

		var argument = CreateArgument("shared@example.com", "new-apple-id");
		var result = await builder.AppleHandler.HandleAsync(argument);

		var appleAuths = await builder.AppleAuthAdapter.GetAllAsync();
		Assert.Equal(seededUser.Id, result.Id);
		Assert.Single(appleAuths);
		Assert.False(result.IsNewUser);
	}

	[Fact]
	public async Task ExistingUser_WithNoRoles_SelfHealsUserRole()
	{
		var builder = new SignInHandlerBuilder();
		var seededUser = await SeedAppleUserAsync(builder);

		var roles = (await builder.UserRoleAdapter.FindAsync(x => x.UserId == seededUser.Id)).ToList();
		foreach (var role in roles)
			await builder.UserRoleAdapter.DeleteAsync(role);

		var argument = CreateArgument("existing@icloud.com", "apple-123");
		var result = await builder.AppleHandler.HandleAsync(argument);

		Assert.Contains(Domain.Framework.Roles.User, result.Roles);
	}

	[Fact]
	public async Task NewUser_AssignsUserRole()
	{
		var builder = new SignInHandlerBuilder();

		var argument = CreateArgument("new@icloud.com", "new-apple-id");
		var result = await builder.AppleHandler.HandleAsync(argument);

		var roles = await builder.UserRoleAdapter.GetAllAsync();
		var role = Assert.Single(roles);
		Assert.Equal(Domain.Framework.Roles.User, role.Role);
	}

	[Fact]
	public async Task NewUser_CreatesUserAndAppleAuth()
	{
		var builder = new SignInHandlerBuilder();

		var argument = CreateArgument("new@icloud.com", "new-apple-id");
		var result = await builder.AppleHandler.HandleAsync(argument);

		Assert.True(result.Id > 0);

		var appleAuths = await builder.AppleAuthAdapter.GetAllAsync();
		var auth = Assert.Single(appleAuths);
		Assert.Equal("new-apple-id", auth.AppleId);
		Assert.Equal("new@icloud.com", auth.Email);
		Assert.Equal(result.Id, auth.Id);
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

	private static async Task<User> SeedAppleUserAsync(SignInHandlerBuilder builder)
	{
		var argument = new SignInArgumentOf<AppleSignInPayload>
		{
			DisplayName = "Existing User", Email = "existing@icloud.com", FirstName = "Existing", LastName = "User",
			Payload = new AppleSignInPayload { AppleId = "apple-123", IsPrivateRelay = false }
		};
		return await builder.AppleHandler.HandleAsync(argument);
	}

	private static async Task<User> SeedGoogleUserAsync(SignInHandlerBuilder builder, string email)
	{
		var argument = new SignInArgumentOf<GoogleSignInPayload>
		{
			DisplayName = "Existing User", Email = email, FirstName = "Existing", LastName = "User",
			Payload = new GoogleSignInPayload { AvatarUrl = "https://example.com/avatar.jpg", EmailVerified = true, GoogleId = "google-123", Locale = "en" }
		};
		return await builder.GoogleHandler.HandleAsync(argument);
	}

	#endregion
}
