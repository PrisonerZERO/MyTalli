namespace My.Talli.Domain.Components.Tokens;

/// <summary>Component</summary>
public interface IShopTokenProtector
{
	#region <Methods>

	string Protect(string plaintext);

	string Unprotect(string ciphertext);

	#endregion
}
