namespace My.Talli.Domain.Components;

using System.Text.Json.Serialization;

/// <summary>Model</summary>
public class EtsyMoney
{
    #region <Properties>

    [JsonPropertyName("amount")]
    public long Amount { get; set; }

    [JsonPropertyName("currency_code")]
    public string CurrencyCode { get; set; } = string.Empty;

    [JsonPropertyName("divisor")]
    public long Divisor { get; set; } = 100;

    #endregion

    #region <Methods>

    public decimal ToDecimal()
    {
        if (Divisor <= 0)
            return 0m;

        return (decimal)Amount / Divisor;
    }

    #endregion
}
