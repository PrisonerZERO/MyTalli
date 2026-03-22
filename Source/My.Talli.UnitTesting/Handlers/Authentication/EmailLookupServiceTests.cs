namespace My.Talli.UnitTesting.Handlers.Authentication;

using My.Talli.UnitTesting.Infrastructure.Builders;

using ENTITIES = Domain.Entities;

/// <summary>Tests</summary>
public class EmailLookupServiceTests
{
	#region <Methods>

	[Fact]
	public async Task FindUserIdByEmailAsync_ChecksGoogleFirst()
	{
		var builder = new SignInHandlerBuilder();

		builder.UserRepository.InsertAsync(new ENTITIES.User
		{
			DisplayName = "Google User", FirstName = "Google", InitialProvider = "Google",
			LastLoginAt = DateTime.UtcNow, LastName = "User", PreferredProvider = "Google", UserPreferences = "{}",
		}).Wait();
		var googleUserId = builder.UserRepository.Store[0].Id;
		builder.GoogleAuthRepository.InsertAsync(new ENTITIES.UserAuthenticationGoogle
		{
			AvatarUrl = "", DisplayName = "Google User", Email = "shared@example.com",
			EmailVerified = true, FirstName = "Google", GoogleId = "google-1",
			Id = googleUserId, LastName = "User", Locale = "en",
		}).Wait();

		builder.UserRepository.InsertAsync(new ENTITIES.User
		{
			DisplayName = "Microsoft User", FirstName = "Microsoft", InitialProvider = "Microsoft",
			LastLoginAt = DateTime.UtcNow, LastName = "User", PreferredProvider = "Microsoft", UserPreferences = "{}",
		}).Wait();
		var microsoftUserId = builder.UserRepository.Store[1].Id;
		builder.MicrosoftAuthRepository.InsertAsync(new ENTITIES.UserAuthenticationMicrosoft
		{
			DisplayName = "Microsoft User", Email = "shared@example.com", FirstName = "Microsoft",
			Id = microsoftUserId, LastName = "User", MicrosoftId = "microsoft-1",
		}).Wait();

		var result = await builder.EmailLookupService.FindUserIdByEmailAsync("shared@example.com");

		Assert.Equal(googleUserId, result);
	}

	[Fact]
	public async Task FindUserIdByEmailAsync_EmailInApple_ReturnsUserId()
	{
		var builder = new SignInHandlerBuilder();

		builder.UserRepository.InsertAsync(new ENTITIES.User
		{
			DisplayName = "Apple User", FirstName = "Apple", InitialProvider = "Apple",
			LastLoginAt = DateTime.UtcNow, LastName = "User", PreferredProvider = "Apple", UserPreferences = "{}",
		}).Wait();
		var userId = builder.UserRepository.Store[0].Id;
		builder.AppleAuthRepository.InsertAsync(new ENTITIES.UserAuthenticationApple
		{
			AppleId = "apple-1", DisplayName = "Apple User", Email = "test@icloud.com",
			FirstName = "Apple", Id = userId, LastName = "User",
		}).Wait();

		var result = await builder.EmailLookupService.FindUserIdByEmailAsync("test@icloud.com");

		Assert.Equal(userId, result);
	}

	[Fact]
	public async Task FindUserIdByEmailAsync_EmailInGoogle_ReturnsUserId()
	{
		var builder = new SignInHandlerBuilder();

		builder.UserRepository.InsertAsync(new ENTITIES.User
		{
			DisplayName = "Google User", FirstName = "Google", InitialProvider = "Google",
			LastLoginAt = DateTime.UtcNow, LastName = "User", PreferredProvider = "Google", UserPreferences = "{}",
		}).Wait();
		var userId = builder.UserRepository.Store[0].Id;
		builder.GoogleAuthRepository.InsertAsync(new ENTITIES.UserAuthenticationGoogle
		{
			AvatarUrl = "", DisplayName = "Google User", Email = "test@gmail.com",
			EmailVerified = true, FirstName = "Google", GoogleId = "google-1",
			Id = userId, LastName = "User", Locale = "en",
		}).Wait();

		var result = await builder.EmailLookupService.FindUserIdByEmailAsync("test@gmail.com");

		Assert.Equal(userId, result);
	}

	[Fact]
	public async Task FindUserIdByEmailAsync_EmailInMicrosoft_ReturnsUserId()
	{
		var builder = new SignInHandlerBuilder();

		builder.UserRepository.InsertAsync(new ENTITIES.User
		{
			DisplayName = "Microsoft User", FirstName = "Microsoft", InitialProvider = "Microsoft",
			LastLoginAt = DateTime.UtcNow, LastName = "User", PreferredProvider = "Microsoft", UserPreferences = "{}",
		}).Wait();
		var userId = builder.UserRepository.Store[0].Id;
		builder.MicrosoftAuthRepository.InsertAsync(new ENTITIES.UserAuthenticationMicrosoft
		{
			DisplayName = "Microsoft User", Email = "test@outlook.com", FirstName = "Microsoft",
			Id = userId, LastName = "User", MicrosoftId = "microsoft-1",
		}).Wait();

		var result = await builder.EmailLookupService.FindUserIdByEmailAsync("test@outlook.com");

		Assert.Equal(userId, result);
	}

	[Fact]
	public async Task FindUserIdByEmailAsync_EmailNotFound_ReturnsNull()
	{
		var builder = new SignInHandlerBuilder();

		var result = await builder.EmailLookupService.FindUserIdByEmailAsync("nobody@example.com");

		Assert.Null(result);
	}

	#endregion
}
