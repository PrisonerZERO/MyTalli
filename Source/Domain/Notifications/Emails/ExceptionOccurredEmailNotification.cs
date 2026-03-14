using System.Net;
using System.Reflection;
using My.Talli.Domain.Extensions;

namespace My.Talli.Domain.Notifications.Emails;

public class ExceptionOccurredEmailNotification : EmailNotificationOf<ExceptionOccurredEmailNotificationPayload>
{
    #region <Variables>

    private const string TemplateResourceName = "My.Talli.Domain..resources.emails.ExceptionOccurredEmailNotificationTemplate.html";

    #endregion

    #region <Methods>

    public override SmtpNotification Build(EmailNotificationArgumentOf<ExceptionOccurredEmailNotificationPayload> arguments)
    {
        var payload = arguments.Payload;

        var template = Assembly.GetExecutingAssembly().GetManifestResourceContent(TemplateResourceName);

        Body = template
            .Replace("[[Environment.Name]]", WebUtility.HtmlEncode(payload.EnvironmentName))
            .Replace("[[Exception.Message]]", WebUtility.HtmlEncode(payload.ExceptionMessage))
            .Replace("[[Exception.StackTrace]]", WebUtility.HtmlEncode(payload.ExceptionStackTrace))
            .Replace("[[Exception.Timestamp]]", WebUtility.HtmlEncode(payload.ExceptionTimestamp))
            .Replace("[[Exception.Type]]", WebUtility.HtmlEncode(payload.ExceptionType))
            .Replace("[[Request.Method]]", WebUtility.HtmlEncode(payload.RequestMethod))
            .Replace("[[Request.Path]]", WebUtility.HtmlEncode(payload.RequestPath));

        Subject = $"[MyTalli] Exception: {payload.ExceptionType}";

        return FinalizeEmail();
    }

    #endregion
}
