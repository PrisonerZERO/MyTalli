namespace My.Talli.UnitTesting.Handlers.Authentication;

using Domain.Handlers.Authentication;
using Domain.Models;
using My.Talli.UnitTesting.Infrastructure.Builders;

using ENTITIES = Domain.Entities;

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

		builder.UserRoleRepository.InsertAsync(new ENTITIES.UserRole
		{
			Role = Domain.Framework.Roles.Admin,
			UserId = firstSignIn.Id,
		}).Wait();

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

		Assert.Equal(1, builder.UserRepository.Store.Count);
		Assert.Equal(googleResult.Id, appleResult.Id);
		Assert.Equal(1, builder.GoogleAuthRepository.Store.Count);
		Assert.Equal(1, builder.AppleAuthRepository.Store.Count);
		Assert.Equal(googleResult.Id, builder.GoogleAuthRepository.Store[0].Id);
		Assert.Equal(googleResult.Id, builder.AppleAuthRepository.Store[0].Id);
		Assert.False(appleResult.IsNewUser);
	}

	[Fact]
	public async Task MultipleUsersMultipleProviders_EmailLookupFindsCorrectUser()
	{
		var builder = new SignInHandlerBuilder();

		var googleArgument = new SignInArgumentOf<GoogleSignInPayload>
		{
			DisplayName = "Google User", Email = "google@example.com", FirstName = "Google", LastName = "User",
			Payload = new GoogleSignInPayload { GoogleId = "google-1", AvatarUrl = "", EmailVerified = true, Locale = "en" }
		};
		var googleUser = await builder.GoogleHandler.HandleAsync(googleArgument);

		var appleArgument = new SignInArgumentOf<AppleSignInPayload>
		{
			DisplayName = "Apple User", Email = "apple@example.com", FirstName = "Apple", LastName = "User",
			Payload = new AppleSignInPayload { AppleId = "apple-1", IsPrivateRelay = false }
		};
		var appleUser = await builder.AppleHandler.HandleAsync(appleArgument);

		var microsoftArgument = new SignInArgumentOf<MicrosoftSignInPayload>
		{
			DisplayName = "Microsoft User", Email = "microsoft@example.com", FirstName = "Microsoft", LastName = "User",
			Payload = new MicrosoftSignInPayload { MicrosoftId = "microsoft-1" }
		};
		var microsoftUser = await builder.MicrosoftHandler.HandleAsync(microsoftArgument);

		Assert.Equal(3, builder.UserRepository.Store.Count);

		var foundGoogle = await builder.EmailLookupService.FindUserIdByEmailAsync("google@example.com");
		var foundApple = await builder.EmailLookupService.FindUserIdByEmailAsync("apple@example.com");
		var foundMicrosoft = await builder.EmailLookupService.FindUserIdByEmailAsync("microsoft@example.com");
		var foundNobody = await builder.EmailLookupService.FindUserIdByEmailAsync("nobody@example.com");

		Assert.Equal(googleUser.Id, foundGoogle);
		Assert.Equal(appleUser.Id, foundApple);
		Assert.Equal(microsoftUser.Id, foundMicrosoft);
		Assert.Null(foundNobody);
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
		var firstLoginAt = first.LastLoginAt;

		var second = await builder.GoogleHandler.HandleAsync(argument);
		var secondLoginAt = second.LastLoginAt;

		var third = await builder.GoogleHandler.HandleAsync(argument);
		var thirdLoginAt = third.LastLoginAt;

		Assert.Equal(1, builder.UserRepository.Store.Count);
		Assert.Equal(1, builder.GoogleAuthRepository.Store.Count);
		Assert.Equal(first.Id, second.Id);
		Assert.Equal(second.Id, third.Id);
		Assert.True(secondLoginAt >= firstLoginAt);
		Assert.True(thirdLoginAt >= secondLoginAt);
		Assert.True(first.IsNewUser);
		Assert.False(second.IsNewUser);
		Assert.False(third.IsNewUser);
	}

	#endregion
}
