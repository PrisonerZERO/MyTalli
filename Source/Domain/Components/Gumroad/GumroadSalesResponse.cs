namespace My.Talli.Domain.Components;

using System.Text.Json.Serialization;

/// <summary>Response</summary>
public class GumroadSalesResponse
{
    #region <Properties>

    [JsonPropertyName("next_page_key")]
    public string? NextPageKey { get; set; }

    [JsonPropertyName("next_page_url")]
    public string? NextPageUrl { get; set; }

    [JsonPropertyName("sales")]
    public List<GumroadSale> Sales { get; set; } = new();

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    #endregion
}
