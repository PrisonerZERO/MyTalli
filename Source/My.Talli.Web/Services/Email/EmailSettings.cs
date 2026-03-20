namespace My.Talli.Web.Services.Email;

/// <summary>Settings</summary>
public class EmailSettings
{
    #region <Properties>

    public List<string> ExceptionRecipients { get; set; } = [];

    public string FromAddress { get; set; } = "DoNotReply@mytalli.com";

    public string FromDisplayName { get; set; } = "MyTalli";

    public string Host { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public string Username { get; set; } = string.Empty;

    public bool UseSsl { get; set; } = true;


    #endregion
}
