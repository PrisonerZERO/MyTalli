namespace My.Talli.Domain.Components;

using System.Text.Json.Serialization;

/// <summary>Model</summary>
public class EtsyReceipt
{
    #region <Properties>

    [JsonPropertyName("create_timestamp")]
    public long CreateTimestamp { get; set; }

    [JsonPropertyName("grandtotal")]
    public EtsyMoney? Grandtotal { get; set; }

    [JsonPropertyName("is_paid")]
    public bool IsPaid { get; set; }

    [JsonPropertyName("is_shipped")]
    public bool IsShipped { get; set; }

    [JsonPropertyName("receipt_id")]
    public long ReceiptId { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("subtotal")]
    public EtsyMoney? Subtotal { get; set; }

    [JsonPropertyName("total_shipping_cost")]
    public EtsyMoney? TotalShippingCost { get; set; }

    [JsonPropertyName("total_tax_cost")]
    public EtsyMoney? TotalTaxCost { get; set; }

    [JsonPropertyName("transactions")]
    public List<EtsyTransaction> Transactions { get; set; } = new();

    [JsonPropertyName("update_timestamp")]
    public long UpdateTimestamp { get; set; }

    #endregion
}
