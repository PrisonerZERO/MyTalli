namespace My.Talli.Domain.Notifications.Emails;

/// <summary>Payload</summary>
public class WeeklySummaryEmailNotificationPayload
{
    #region <Properties>

    public string FirstName { get; set; } = string.Empty;

    public string GoalCurrent { get; set; } = string.Empty;

    public string GoalPercent { get; set; } = string.Empty;

    public string GoalRemaining { get; set; } = string.Empty;

    public string GoalTarget { get; set; } = string.Empty;

    public string PlatformRows { get; set; } = string.Empty;

    public string TotalRevenue { get; set; } = string.Empty;

    public string TrendDirection { get; set; } = string.Empty;

    public string TrendPercent { get; set; } = string.Empty;

    public string UnsubscribeToken { get; set; } = string.Empty;

    public string WeekRange { get; set; } = string.Empty;


    #endregion
}
