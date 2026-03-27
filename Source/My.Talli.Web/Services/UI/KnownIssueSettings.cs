namespace My.Talli.Web.Services.UI;

/// <summary>Settings</summary>
public class KnownIssueSettings
{
    #region <Properties>

    public bool IsActive { get; set; } = false;

    public string Message { get; set; } = string.Empty;

    public string Severity { get; set; } = "Warning";

    #endregion
}
