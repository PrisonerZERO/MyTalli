namespace My.Talli.Web.Services.Billing;

/// <summary>Settings</summary>
public class StripeSettings
{
    #region <Properties>

    public string MonthlyPriceId { get; set; } = string.Empty;

    public Dictionary<string, string> Modules { get; set; } = new();

    public string PortalConfigurationId { get; set; } = string.Empty;

    public string PublishableKey { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    public string WebhookSecret { get; set; } = string.Empty;

    public string YearlyPriceId { get; set; } = string.Empty;


    #endregion
}
