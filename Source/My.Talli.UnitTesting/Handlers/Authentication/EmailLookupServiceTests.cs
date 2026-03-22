namespace My.Talli.UnitTesting.Handlers.Authentication;

using Domain.Handlers.Authentication;
using Domain.Models;
using My.Talli.UnitTesting.Infrastructure.Builders;

/// <summary>Tests</summary>
public class EmailLookupServiceTests
{
	#region <Methods>

	[Fact]
	public async Task FindUserIdByEmailAsync_ChecksGoogleFirst()
	{
		var builder = new SignInHandlerBuilder();

		var googleUser = await SeedGoogleUserAsync(builder, "shared@example.com");
		await SeedMicrosoftUserAsync(builder, "shared@example.com");

		var result = await builder.EmailLookupService.FindUserIdByEmailAsync("shared@example.com");

		Assert.Equal(googleUser.Id, result);
	}

	[Fact]
	public async Task FindUserIdByEmailAsync_EmailInApple_ReturnsUserId()
	{
		var builder = new SignInHandlerBuilder();
		var appleUser = await SeedAppleUserAsync(builder, "test@icloud.com");

		var result = await builder.EmailLookupService.FindUserIdByEmailAsync("test@icloud.com");

		Assert.Equal(appleUser.Id, result);
	}

	[Fact]
	public async Task FindUserIdByEmailAsync_EmailInGoogle_ReturnsUserId()
	{
		var builder = new SignInHandlerBuilder();
		var googleUser = await SeedGoogleUserAsync(builder, "test@gmail.com");

		var result = await builder.EmailLookupService.FindUserIdByEmailAsync("test@gmail.com");

		Assert.Equal(googleUser.Id, result);
	}

	[Fact]
	public async Task FindUserIdByEmailAsync_EmailInMicrosoft_ReturnsUserId()
	{
		var builder = new SignInHandlerBuilder();
		var microsoftUser = await SeedMicrosoftUserAsync(builder, "test@outlook.com");

		var result = await builder.EmailLookupService.FindUserIdByEmailAsync("test@outlook.com");

		Assert.Equal(microsoftUser.Id, result);
	}

	[Fact]
	public async Task FindUserIdByEmailAsync_EmailNotFound_ReturnsNull()
	{
		var builder = new SignInHandlerBuilder();

		var result = await builder.EmailLookupService.FindUserIdByEmailAsync("nobody@example.com");

		Assert.Null(result);
	}

	private static async Task<User> SeedAppleUserAsync(SignInHandlerBuilder builder, string email)
	{
		return await builder.AppleHandler.HandleAsync(new SignInArgumentOf<AppleSignInPayload>
		{
			DisplayName = "Apple User", Email = email, FirstName = "Apple", LastName = "User",
			Payload = new AppleSignInPayload { AppleId = "apple-1", IsPrivateRelay = false }
		});
	}

	private static async Task<User> SeedGoogleUserAsync(SignInHandlerBuilder builder, string email)
	{
		return await builder.GoogleHandler.HandleAsync(new SignInArgumentOf<GoogleSignInPayload>
		{
			DisplayName = "Google User", Email = email, FirstName = "Google", LastName = "User",
			Payload = new GoogleSignInPayload { AvatarUrl = "", EmailVerified = true, GoogleId = $"google-{email}", Locale = "en" }
		});
	}

	private static async Task<User> SeedMicrosoftUserAsync(SignInHandlerBuilder builder, string email)
	{
		return await builder.MicrosoftHandler.HandleAsync(new SignInArgumentOf<MicrosoftSignInPayload>
		{
			DisplayName = "Microsoft User", Email = email, FirstName = "Microsoft", LastName = "User",
			Payload = new MicrosoftSignInPayload { MicrosoftId = $"microsoft-{email}" }
		});
	}

	#endregion
}
