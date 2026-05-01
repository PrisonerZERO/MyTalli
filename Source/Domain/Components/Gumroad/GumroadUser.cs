namespace My.Talli.Domain.Components;

using System.Text.Json.Serialization;

/// <summary>Model</summary>
public class GumroadUser
{
    #region <Properties>

    [JsonPropertyName("currency_type")]
    public string? CurrencyType { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }

    #endregion
}
