namespace My.Talli.UnitTesting.Handlers.Authentication;

using Domain.Handlers.Authentication;
using Domain.Models;
using My.Talli.UnitTesting.Infrastructure.Builders;

/// <summary>Tests</summary>
public class MicrosoftSignInHandlerTests
{
	#region <Methods>

	[Fact]
	public async Task ExistingEmailOnGoogle_LinksMicrosoftAuth()
	{
		var builder = new SignInHandlerBuilder();
		var seededUser = await SeedGoogleUserAsync(builder, "shared@example.com");

		var argument = CreateArgument("shared@example.com", "new-microsoft-id");
		var result = await builder.MicrosoftHandler.HandleAsync(argument);

		var microsoftAuths = await builder.MicrosoftAuthAdapter.GetAllAsync();
		Assert.Equal(seededUser.Id, result.Id);
		Assert.Single(microsoftAuths);
		Assert.False(result.IsNewUser);
	}

	[Fact]
	public async Task ExistingMicrosoftUser_ReturnsExistingUser()
	{
		var builder = new SignInHandlerBuilder();
		var seededUser = await SeedMicrosoftUserAsync(builder);

		var argument = CreateArgument("existing@outlook.com", "microsoft-123");
		var result = await builder.MicrosoftHandler.HandleAsync(argument);

		Assert.Equal(seededUser.Id, result.Id);
		Assert.False(result.IsNewUser);
	}

	[Fact]
	public async Task ExistingMicrosoftUser_UpdatesLastLoginAt()
	{
		var builder = new SignInHandlerBuilder();
		var seededUser = await SeedMicrosoftUserAsync(builder);

		var argument = CreateArgument("existing@outlook.com", "microsoft-123");
		var result = await builder.MicrosoftHandler.HandleAsync(argument);

		Assert.True(result.LastLoginAt > seededUser.LastLoginAt);
	}

	[Fact]
	public async Task ExistingUser_WithNoRoles_SelfHealsUserRole()
	{
		var builder = new SignInHandlerBuilder();
		var seededUser = await SeedMicrosoftUserAsync(builder);

		var roles = (await builder.UserRoleAdapter.FindAsync(x => x.UserId == seededUser.Id)).ToList();
		foreach (var role in roles)
			await builder.UserRoleAdapter.DeleteAsync(role);

		var argument = CreateArgument("existing@outlook.com", "microsoft-123");
		var result = await builder.MicrosoftHandler.HandleAsync(argument);

		Assert.Contains(Domain.Framework.Roles.User, result.Roles);
	}

	[Fact]
	public async Task NewUser_AssignsUserRole()
	{
		var builder = new SignInHandlerBuilder();

		var argument = CreateArgument("new@outlook.com", "new-microsoft-id");
		var result = await builder.MicrosoftHandler.HandleAsync(argument);

		var roles = await builder.UserRoleAdapter.GetAllAsync();
		var role = Assert.Single(roles);
		Assert.Equal(Domain.Framework.Roles.User, role.Role);
	}

	[Fact]
	public async Task NewUser_CreatesUserAndMicrosoftAuth()
	{
		var builder = new SignInHandlerBuilder();

		var argument = CreateArgument("new@outlook.com", "new-microsoft-id");
		var result = await builder.MicrosoftHandler.HandleAsync(argument);

		Assert.True(result.Id > 0);

		var microsoftAuths = await builder.MicrosoftAuthAdapter.GetAllAsync();
		var auth = Assert.Single(microsoftAuths);
		Assert.Equal("new-microsoft-id", auth.MicrosoftId);
		Assert.Equal("new@outlook.com", auth.Email);
		Assert.Equal(result.Id, auth.Id);
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

	private static async Task<User> SeedGoogleUserAsync(SignInHandlerBuilder builder, string email)
	{
		var argument = new SignInArgumentOf<GoogleSignInPayload>
		{
			DisplayName = "Existing User", Email = email, FirstName = "Existing", LastName = "User",
			Payload = new GoogleSignInPayload { AvatarUrl = "https://example.com/avatar.jpg", EmailVerified = true, GoogleId = "google-123", Locale = "en" }
		};
		return await builder.GoogleHandler.HandleAsync(argument);
	}

	private static async Task<User> SeedMicrosoftUserAsync(SignInHandlerBuilder builder)
	{
		var argument = new SignInArgumentOf<MicrosoftSignInPayload>
		{
			DisplayName = "Existing User", Email = "existing@outlook.com", FirstName = "Existing", LastName = "User",
			Payload = new MicrosoftSignInPayload { MicrosoftId = "microsoft-123" }
		};
		return await builder.MicrosoftHandler.HandleAsync(argument);
	}

	#endregion
}
