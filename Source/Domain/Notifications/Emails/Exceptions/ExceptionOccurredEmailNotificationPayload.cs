namespace My.Talli.Domain.Notifications.Emails;

/// <summary>Payload</summary>
public class ExceptionOccurredEmailNotificationPayload
{
    #region <Properties>

    public string EnvironmentName { get; set; } = string.Empty;

    public string ExceptionMessage { get; set; } = string.Empty;

    public string ExceptionStackTrace { get; set; } = string.Empty;

    public string ExceptionTimestamp { get; set; } = string.Empty;

    public string ExceptionType { get; set; } = string.Empty;

    public string RequestMethod { get; set; } = string.Empty;

    public string RequestPath { get; set; } = string.Empty;

    #endregion
}
