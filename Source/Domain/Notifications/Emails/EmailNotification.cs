namespace My.Talli.Domain.Notifications.Emails;

/// <summary>Notification</summary>
public abstract class EmailNotification
{
    #region <Properties>

    public List<string> Bcc { get; set; } = [];

    public string Body { get; set; } = string.Empty;

    public List<string> Cc { get; set; } = [];

    public string From { get; set; } = string.Empty;

    public bool IsBodyHtml { get; set; } = true;

    public string Subject { get; set; } = string.Empty;

    public List<string> To { get; set; } = [];

    #endregion

    #region <Methods>

    protected SmtpNotification FinalizeEmail()
    {
        return new SmtpNotification
        {
            Bcc = Bcc,
            Body = Body,
            Cc = Cc,
            From = From,
            IsBodyHtml = IsBodyHtml,
            Subject = Subject,
            To = To
        };
    }

    #endregion
}
