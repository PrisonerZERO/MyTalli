namespace My.Talli.Web.Services.Tokens;

using Domain.Components.Tokens;
using Microsoft.AspNetCore.DataProtection;

/// <summary>Protector</summary>
public class DataProtectionShopTokenProtector : IShopTokenProtector
{
	#region <Constants>

	private const string Purpose = "MyTalli.ShopConnection.Tokens.v1";

	#endregion

	#region <Variables>

	private readonly IDataProtector _protector;

	#endregion

	#region <Constructors>

	public DataProtectionShopTokenProtector(IDataProtectionProvider provider)
	{
		_protector = provider.CreateProtector(Purpose);
	}

	#endregion

	#region <Methods>

	public string Protect(string plaintext)
	{
		return _protector.Protect(plaintext);
	}

	public string Unprotect(string ciphertext)
	{
		return _protector.Unprotect(ciphertext);
	}

	#endregion
}
