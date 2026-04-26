namespace My.Talli.UnitTesting.Infrastructure.Stubs;

using Domain.Components.Tokens;

/// <summary>Stub</summary>
public class ShopTokenProtectorStub : IShopTokenProtector
{
	#region <Methods>

	public string Protect(string plaintext)
	{
		return $"protected:{plaintext}";
	}

	public string Unprotect(string ciphertext)
	{
		const string prefix = "protected:";
		return ciphertext.StartsWith(prefix) ? ciphertext[prefix.Length..] : ciphertext;
	}

	#endregion
}
