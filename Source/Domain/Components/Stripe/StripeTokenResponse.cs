namespace My.Talli.Domain.Components;

using System.Text.Json.Serialization;

/// <summary>Response</summary>
public class StripeTokenResponse
{
    #region <Properties>

    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("livemode")]
    public bool LiveMode { get; set; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;

    [JsonPropertyName("stripe_publishable_key")]
    public string? StripePublishableKey { get; set; }

    [JsonPropertyName("stripe_user_id")]
    public string StripeUserId { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    #endregion
}
