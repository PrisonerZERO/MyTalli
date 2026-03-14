namespace My.Talli.Domain.Notifications.Emails;

using System.Net;
using System.Reflection;
using My.Talli.Domain.Extensions;

public class WelcomeEmailNotification : EmailNotificationOf<WelcomeEmailNotificationPayload>
{
    #region <Variables>

    private const string TemplateResourceName = "My.Talli.Domain..resources.emails.WelcomeEmailNotificationTemplate.html";

    #endregion

    #region <Methods>

    public override SmtpNotification Build(EmailNotificationArgumentOf<WelcomeEmailNotificationPayload> arguments)
    {
        var payload = arguments.Payload;

        var template = Assembly.GetExecutingAssembly().GetManifestResourceContent(TemplateResourceName);

        Body = template
            .Replace("[[User.FirstName]]", WebUtility.HtmlEncode(payload.FirstName));

        Subject = $"Welcome to MyTalli, {payload.FirstName}!";

        return FinalizeEmail();
    }

    #endregion
}
