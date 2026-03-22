namespace My.Talli.UnitTesting.Components.Tokens;

using Domain.Components.Tokens;

/// <summary>Tests</summary>
public class UnsubscribeTokenServiceTests
{
	#region <Variables>

	private readonly UnsubscribeTokenService _service = new("test-secret-key-for-unit-tests");

	#endregion

	#region <Methods>

	[Fact]
	public void Constructor_EmptySecretKey_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => new UnsubscribeTokenService(""));
	}

	[Fact]
	public void Constructor_NullSecretKey_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => new UnsubscribeTokenService(null!));
	}

	[Fact]
	public void Constructor_WhitespaceSecretKey_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => new UnsubscribeTokenService("   "));
	}

	[Fact]
	public void GenerateToken_NegativeUserId_ThrowsArgumentException()
	{
		Assert.Throws<ArgumentException>(() => _service.GenerateToken(-1));
	}

	[Fact]
	public void GenerateToken_ValidUserId_ReturnsNonEmptyString()
	{
		var token = _service.GenerateToken(42);

		Assert.False(string.IsNullOrWhiteSpace(token));
	}

	[Fact]
	public void GenerateToken_ZeroUserId_ThrowsArgumentException()
	{
		Assert.Throws<ArgumentException>(() => _service.GenerateToken(0));
	}

	[Fact]
	public void ValidateToken_DifferentSecretKey_ReturnsNull()
	{
		var token = _service.GenerateToken(42);
		var otherService = new UnsubscribeTokenService("different-secret-key");

		var result = otherService.ValidateToken(token);

		Assert.Null(result);
	}

	[Fact]
	public void ValidateToken_EmptyToken_ReturnsNull()
	{
		var result = _service.ValidateToken("");

		Assert.Null(result);
	}

	[Fact]
	public void ValidateToken_NullToken_ReturnsNull()
	{
		var result = _service.ValidateToken(null);

		Assert.Null(result);
	}

	[Theory]
	[InlineData(1)]
	[InlineData(42)]
	[InlineData(long.MaxValue)]
	public void ValidateToken_RoundTrip_ReturnsOriginalUserId(long userId)
	{
		var token = _service.GenerateToken(userId);
		var result = _service.ValidateToken(token);

		Assert.Equal(userId, result);
	}

	[Fact]
	public void ValidateToken_TamperedToken_ReturnsNull()
	{
		var token = _service.GenerateToken(42);
		var tampered = token[..^1] + (token[^1] == 'A' ? 'B' : 'A');

		var result = _service.ValidateToken(tampered);

		Assert.Null(result);
	}

	#endregion
}
