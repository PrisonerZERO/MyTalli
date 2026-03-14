namespace My.Talli.Domain.Notifications.Emails;

/// <summary>Notification</summary>
public class SmtpNotification
{
    #region <Properties>

    public List<string> Bcc { get; set; } = [];

    public string Body { get; set; } = string.Empty;

    public List<string> Cc { get; set; } = [];

    public string From { get; set; } = string.Empty;

    public bool IsBodyHtml { get; set; }

    public string Subject { get; set; } = string.Empty;

    public List<string> To { get; set; } = [];

    #endregion
}
