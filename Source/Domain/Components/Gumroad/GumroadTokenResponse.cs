namespace My.Talli.Domain.Components;

using System.Text.Json.Serialization;

/// <summary>Response</summary>
public class GumroadTokenResponse
{
    #region <Properties>

    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    #endregion
}
