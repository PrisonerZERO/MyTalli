namespace My.Talli.Domain.Components;

using System.Security.Cryptography;
using System.Text;

/// <summary>Generator</summary>
public static class EtsyPkceGenerator
{
    #region <Constants>

    public const string AuthorizeUrl = "https://www.etsy.com/oauth/connect";

    #endregion

    #region <Methods>

    public static AuthorizeChallenge BuildAuthorizeChallenge(string clientId, string redirectUri, string scope)
    {
        var codeVerifier = GenerateCodeVerifier();
        var codeChallenge = GenerateCodeChallenge(codeVerifier);
        var state = GenerateState();

        var authorizeUrl =
            $"{AuthorizeUrl}" +
            $"?response_type=code" +
            $"&client_id={Uri.EscapeDataString(clientId)}" +
            $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
            $"&scope={Uri.EscapeDataString(scope)}" +
            $"&state={Uri.EscapeDataString(state)}" +
            $"&code_challenge={Uri.EscapeDataString(codeChallenge)}" +
            $"&code_challenge_method=S256";

        return new AuthorizeChallenge
        {
            AuthorizeUrl = authorizeUrl,
            CodeVerifier = codeVerifier,
            State = state
        };
    }

    public static string ExtractEtsyUserId(string accessToken)
    {
        if (string.IsNullOrEmpty(accessToken))
            return string.Empty;

        var dot = accessToken.IndexOf('.');
        return dot > 0 ? accessToken[..dot] : string.Empty;
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        var bytes = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
        return Base64UrlEncode(bytes);
    }

    private static string GenerateCodeVerifier()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Base64UrlEncode(bytes);
    }

    private static string GenerateState()
    {
        var bytes = RandomNumberGenerator.GetBytes(16);
        return Base64UrlEncode(bytes);
    }

    #endregion
}
