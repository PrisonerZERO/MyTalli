namespace My.Talli.Domain.Components.Tokens;

using Domain.Framework;
using System.Security.Cryptography;
using System.Text;

/// <summary>Component</summary>
public class UnsubscribeTokenService
{
	#region <Variables>

	private readonly byte[] _secretKey;


	#endregion

	#region <Constructors>

	public UnsubscribeTokenService(string secretKey)
	{
		Assert.IsNullOrWhitespace(secretKey, nameof(secretKey));

		_secretKey = Encoding.UTF8.GetBytes(secretKey);
	}


	#endregion

	#region <Methods>

	public string GenerateToken(long userId)
	{
		Assert.IsValidIdentity(userId, nameof(userId));

		var payload = userId.ToString();
		var signature = ComputeSignature(payload);

		return Base64UrlEncode($"{payload}.{signature}");
	}

	public long? ValidateToken(string? token)
	{
		if (string.IsNullOrWhiteSpace(token))
			return null;

		try
		{
			var decoded = Base64UrlDecode(token);
			var separatorIndex = decoded.IndexOf('.');

			if (separatorIndex < 0)
				return null;

			var payload = decoded[..separatorIndex];
			var signature = decoded[(separatorIndex + 1)..];
			var expectedSignature = ComputeSignature(payload);

			if (!CryptographicOperations.FixedTimeEquals(
				Encoding.UTF8.GetBytes(signature),
				Encoding.UTF8.GetBytes(expectedSignature)))
				return null;

			return long.TryParse(payload, out var userId) ? userId : null;
		}
		catch
		{
			return null;
		}
	}

	private static string Base64UrlDecode(string input)
	{
		var base64 = input.Replace('-', '+').Replace('_', '/');

		switch (base64.Length % 4)
		{
			case 2: base64 += "=="; break;
			case 3: base64 += "="; break;
		}

		return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
	}

	private static string Base64UrlEncode(string input)
	{
		return Convert.ToBase64String(Encoding.UTF8.GetBytes(input))
			.TrimEnd('=')
			.Replace('+', '-')
			.Replace('/', '_');
	}

	private string ComputeSignature(string payload)
	{
		using var hmac = new HMACSHA256(_secretKey);
		var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
		return Convert.ToBase64String(hash);
	}


	#endregion
}
