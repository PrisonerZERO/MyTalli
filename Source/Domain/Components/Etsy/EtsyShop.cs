namespace My.Talli.Domain.Components;

using System.Text.Json.Serialization;

/// <summary>Model</summary>
public class EtsyShop
{
    #region <Properties>

    [JsonPropertyName("shipping_from_country_iso")]
    public string? CountryIso { get; set; }

    [JsonPropertyName("currency_code")]
    public string? CurrencyCode { get; set; }

    [JsonPropertyName("is_vacation")]
    public bool IsVacation { get; set; }

    [JsonPropertyName("shop_id")]
    public long ShopId { get; set; }

    [JsonPropertyName("shop_name")]
    public string? ShopName { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    #endregion
}
