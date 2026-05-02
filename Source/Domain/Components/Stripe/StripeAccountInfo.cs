namespace My.Talli.Domain.Components;

/// <summary>Model</summary>
public class StripeAccountInfo
{
    #region <Properties>

    public string AccountId { get; set; } = string.Empty;

    public string BusinessName { get; set; } = string.Empty;

    public string? Email { get; set; }

    #endregion
}
