namespace My.Talli.Domain.Components;

using System.Text.Json.Serialization;

/// <summary>Model</summary>
public class EtsyTransaction
{
    #region <Properties>

    [JsonPropertyName("create_timestamp")]
    public long CreateTimestamp { get; set; }

    [JsonPropertyName("is_digital")]
    public bool IsDigital { get; set; }

    [JsonPropertyName("listing_id")]
    public long ListingId { get; set; }

    [JsonPropertyName("paid_timestamp")]
    public long? PaidTimestamp { get; set; }

    [JsonPropertyName("price")]
    public EtsyMoney? Price { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("shipping_cost")]
    public EtsyMoney? ShippingCost { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("transaction_id")]
    public long TransactionId { get; set; }

    #endregion
}
