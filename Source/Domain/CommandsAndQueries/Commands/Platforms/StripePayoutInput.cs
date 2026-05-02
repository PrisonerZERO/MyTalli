namespace My.Talli.Domain.Commands.Platforms;

/// <summary>Input</summary>
public class StripePayoutInput
{
    #region <Properties>

    public decimal Amount { get; set; }

    public string Currency { get; set; } = string.Empty;

    public DateTime? ExpectedArrivalDate { get; set; }

    public DateTime PayoutDate { get; set; }

    public string PayoutMethod { get; set; } = string.Empty;

    public string? StatementDescriptor { get; set; }

    public string Status { get; set; } = string.Empty;

    public string StripePayoutId { get; set; } = string.Empty;

    #endregion
}
