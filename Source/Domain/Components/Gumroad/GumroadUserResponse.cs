namespace My.Talli.Domain.Components;

using System.Text.Json.Serialization;

/// <summary>Response</summary>
public class GumroadUserResponse
{
    #region <Properties>

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("user")]
    public GumroadUser? User { get; set; }

    #endregion
}
