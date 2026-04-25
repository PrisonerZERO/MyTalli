namespace My.Talli.Domain.Components;

using System.Text.Json.Serialization;

/// <summary>Model</summary>
public class EtsyLedgerEntry
{
    #region <Properties>

    [JsonPropertyName("amount"), JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal Amount { get; set; }

    [JsonPropertyName("balance"), JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal Balance { get; set; }

    [JsonPropertyName("create_date")]
    public long CreateDate { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("entry_id")]
    public long EntryId { get; set; }

    [JsonPropertyName("ledger_entry_type")]
    public string? LedgerEntryType { get; set; }

    [JsonPropertyName("ledger_id")]
    public long LedgerId { get; set; }

    [JsonPropertyName("reference_id")]
    public long? ReferenceId { get; set; }

    [JsonPropertyName("sequence_number")]
    public int SequenceNumber { get; set; }

    #endregion
}
