namespace My.Talli.Web.Services.Platforms;

/// <summary>Settings</summary>
public class StripeConnectSettings
{
    #region <Properties>

    public string ClientId { get; set; } = string.Empty;

    public string RedirectUri { get; set; } = string.Empty;

    public string Scope { get; set; } = "read_only";

    #endregion
}
