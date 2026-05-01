namespace My.Talli.Domain.Components;

using System.Text.Json.Serialization;

/// <summary>Model</summary>
public class GumroadSale
{
    #region <Properties>

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("discover_fee_charged")]
    public bool DiscoverFeeCharged { get; set; }

    [JsonPropertyName("disputed")]
    public bool Disputed { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("license_key")]
    public string? LicenseKey { get; set; }

    [JsonPropertyName("price")]
    public long PriceCents { get; set; }

    [JsonPropertyName("product_id")]
    public string? ProductId { get; set; }

    [JsonPropertyName("product_name")]
    public string? ProductName { get; set; }

    [JsonPropertyName("refunded")]
    public bool Refunded { get; set; }

    #endregion
}
