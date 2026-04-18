namespace My.Talli.Domain.Components;

using System.Text.Json.Serialization;

/// <summary>Response</summary>
public class EtsyTokenResponse
{
    #region <Properties>

    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    #endregion
}
