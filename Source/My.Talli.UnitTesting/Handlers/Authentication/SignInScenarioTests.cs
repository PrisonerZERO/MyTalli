namespace My.Talli.UnitTesting.Handlers.Authentication;

using Domain.Handlers.Authentication;
using Domain.Models;
using My.Talli.UnitTesting.Infrastructure.Builders;

/// <summary>Tests</summary>
public class SignInScenarioTests
{
	#region <Methods>

	[Fact]
	public async Task AdminAndUserRoles_BothReturnOnSignIn()
	{
		var builder = new SignInHandlerBuilder();

		var argument = new SignInArgumentOf<GoogleSignInPayload>
		{
			DisplayName = "Admin User", Email = "admin@gmail.com", FirstName = "Admin", LastName = "User",
			Payload = new GoogleSignInPayload { GoogleId = "google-admin", AvatarUrl = "", EmailVerified = true, Locale = "en" }
		};
		var firstSignIn = await builder.GoogleHandler.HandleAsync(argument);

		await builder.UserRoleAdapter.InsertAsync(new UserRole
		{
			Role = Domain.Framework.Roles.Admin,
			UserId = firstSignIn.Id,
		});

		var secondSignIn = await builder.GoogleHandler.HandleAsync(argument);

		Assert.Equal(2, secondSignIn.Roles.Count);
		Assert.Contains(Domain.Framework.Roles.User, secondSignIn.Roles);
		Assert.Contains(Domain.Framework.Roles.Admin, secondSignIn.Roles);
	}

	[Fact]
	public async Task GoogleThenAppleSameEmail_SingleAccountBothProvidersLinked()
	{
		var builder = new SignInHandlerBuilder();

		var googleArgument = new SignInArgumentOf<GoogleSignInPayload>
		{
			DisplayName = "Test User", Email = "shared@example.com", FirstName = "Test", LastName = "User",
			Payload = new GoogleSignInPayload { GoogleId = "google-1", AvatarUrl = "https://example.com/avatar.jpg", EmailVerified = true, Locale = "en" }
		};
		var googleResult = await builder.GoogleHandler.HandleAsync(googleArgument);

		var appleArgument = new SignInArgumentOf<AppleSignInPayload>
		{
			DisplayName = "Test User", Email = "shared@example.com", FirstName = "Test", LastName = "User",
			Payload = new AppleSignInPayload { AppleId = "apple-1", IsPrivateRelay = false }
		};
		var appleResult = await builder.AppleHandler.HandleAsync(appleArgument);

		var users = await builder.UserAdapter.GetAllAsync();
		var googleAuths = await builder.GoogleAuthAdapter.GetAllAsync();
		var appleAuths = await builder.AppleAuthAdapter.GetAllAsync();

		Assert.Single(users);
		Assert.Equal(googleResult.Id, appleResult.Id);
		Assert.Single(googleAuths);
		Assert.Single(appleAuths);
		Assert.False(appleResult.IsNewUser);
	}

	[Fact]
	public async Task MultipleUsersMultipleProviders_EmailLookupFindsCorrectUser()
	{
		var builder = new SignInHandlerBuilder();

		var googleUser = await builder.GoogleHandler.HandleAsync(new SignInArgumentOf<GoogleSignInPayload>
		{
			DisplayName = "Google User", Email = "google@example.com", FirstName = "Google", LastName = "User",
			Payload = new GoogleSignInPayload { GoogleId = "google-1", AvatarUrl = "", EmailVerified = true, Locale = "en" }
		});

		var appleUser = await builder.AppleHandler.HandleAsync(new SignInArgumentOf<AppleSignInPayload>
		{
			DisplayName = "Apple User", Email = "apple@example.com", FirstName = "Apple", LastName = "User",
			Payload = new AppleSignInPayload { AppleId = "apple-1", IsPrivateRelay = false }
		});

		var microsoftUser = await builder.MicrosoftHandler.HandleAsync(new SignInArgumentOf<MicrosoftSignInPayload>
		{
			DisplayName = "Microsoft User", Email = "microsoft@example.com", FirstName = "Microsoft", LastName = "User",
			Payload = new MicrosoftSignInPayload { MicrosoftId = "microsoft-1" }
		});

		var users = await builder.UserAdapter.GetAllAsync();
		Assert.Equal(3, users.Count());

		Assert.Equal(googleUser.Id, await builder.EmailLookupService.FindUserIdByEmailAsync("google@example.com"));
		Assert.Equal(appleUser.Id, await builder.EmailLookupService.FindUserIdByEmailAsync("apple@example.com"));
		Assert.Equal(microsoftUser.Id, await builder.EmailLookupService.FindUserIdByEmailAsync("microsoft@example.com"));
		Assert.Null(await builder.EmailLookupService.FindUserIdByEmailAsync("nobody@example.com"));
	}

	[Fact]
	public async Task RepeatedSignIns_NoDuplicates_LastLoginAtKeepsUpdating()
	{
		var builder = new SignInHandlerBuilder();

		var argument = new SignInArgumentOf<GoogleSignInPayload>
		{
			DisplayName = "Test User", Email = "repeat@gmail.com", FirstName = "Test", LastName = "User",
			Payload = new GoogleSignInPayload { GoogleId = "google-repeat", AvatarUrl = "", EmailVerified = true, Locale = "en" }
		};

		var first = await builder.GoogleHandler.HandleAsync(argument);
		var second = await builder.GoogleHandler.HandleAsync(argument);
		var third = await builder.GoogleHandler.HandleAsync(argument);

		var users = await builder.UserAdapter.GetAllAsync();
		var googleAuths = await builder.GoogleAuthAdapter.GetAllAsync();

		Assert.Single(users);
		Assert.Single(googleAuths);
		Assert.Equal(first.Id, second.Id);
		Assert.Equal(second.Id, third.Id);
		Assert.True(second.LastLoginAt >= first.LastLoginAt);
		Assert.True(third.LastLoginAt >= second.LastLoginAt);
		Assert.True(first.IsNewUser);
		Assert.False(second.IsNewUser);
		Assert.False(third.IsNewUser);
	}

	#endregion
}
