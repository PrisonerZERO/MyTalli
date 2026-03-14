namespace My.Talli.Domain.Notifications.Emails;

using System.Net;
using System.Reflection;
using Domain.Extensions;

/// <summary>Notification</summary>
public class WeeklySummaryEmailNotification : EmailNotificationOf<WeeklySummaryEmailNotificationPayload>
{
    #region <Variables>

    private const string TemplateResourceName = "My.Talli.Domain..resources.emails.WeeklySummaryEmailNotificationTemplate.html";

    #endregion

    #region <Methods>

    public override SmtpNotification Build(EmailNotificationArgumentOf<WeeklySummaryEmailNotificationPayload> arguments)
    {
        var payload = arguments.Payload;

        var template = Assembly.GetExecutingAssembly().GetManifestResourceContent(TemplateResourceName);

        Body = template
            .Replace("[[Summary.GoalCurrent]]", WebUtility.HtmlEncode(payload.GoalCurrent))
            .Replace("[[Summary.GoalPercent]]", WebUtility.HtmlEncode(payload.GoalPercent))
            .Replace("[[Summary.GoalRemaining]]", WebUtility.HtmlEncode(payload.GoalRemaining))
            .Replace("[[Summary.GoalTarget]]", WebUtility.HtmlEncode(payload.GoalTarget))
            .Replace("[[Summary.PlatformRows]]", payload.PlatformRows)
            .Replace("[[Summary.TotalRevenue]]", WebUtility.HtmlEncode(payload.TotalRevenue))
            .Replace("[[Summary.TrendDirection]]", WebUtility.HtmlEncode(payload.TrendDirection))
            .Replace("[[Summary.TrendPercent]]", WebUtility.HtmlEncode(payload.TrendPercent))
            .Replace("[[Summary.WeekRange]]", WebUtility.HtmlEncode(payload.WeekRange))
            .Replace("[[User.FirstName]]", WebUtility.HtmlEncode(payload.FirstName));

        Subject = $"Your Week in Review \u2014 {payload.TotalRevenue} \u2014 MyTalli";

        return FinalizeEmail();
    }

    #endregion
}
