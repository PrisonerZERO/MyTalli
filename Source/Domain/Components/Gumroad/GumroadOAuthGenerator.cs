namespace My.Talli.Domain.Components;

using System.Security.Cryptography;

/// <summary>Generator</summary>
public static class GumroadOAuthGenerator
{
    #region <Constants>

    public const string AuthorizeUrl = "https://gumroad.com/oauth/authorize";

    #endregion

    #region <Methods>

    public static AuthorizeChallenge BuildAuthorizeChallenge(string clientId, string redirectUri, string scope)
    {
        var state = GenerateState();

        var authorizeUrl =
            $"{AuthorizeUrl}" +
            $"?response_type=code" +
            $"&client_id={Uri.EscapeDataString(clientId)}" +
            $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
            $"&scope={Uri.EscapeDataString(scope)}" +
            $"&state={Uri.EscapeDataString(state)}";

        return new AuthorizeChallenge
        {
            AuthorizeUrl = authorizeUrl,
            CodeVerifier = string.Empty,
            State = state
        };
    }

    private static string GenerateState()
    {
        var bytes = RandomNumberGenerator.GetBytes(16);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    #endregion
}
