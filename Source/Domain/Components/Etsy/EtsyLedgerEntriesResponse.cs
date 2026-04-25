namespace My.Talli.Domain.Components;

using System.Text.Json.Serialization;

/// <summary>Response</summary>
public class EtsyLedgerEntriesResponse
{
    #region <Properties>

    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("results")]
    public List<EtsyLedgerEntry> Results { get; set; } = new();

    #endregion
}
