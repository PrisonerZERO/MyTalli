namespace My.Talli.Domain.Notifications.Emails;

using System.Net;
using System.Reflection;
using Domain.Extensions;

/// <summary>Notification</summary>
public class SubscriptionConfirmationEmailNotification : EmailNotificationOf<SubscriptionConfirmationEmailNotificationPayload>
{
    #region <Variables>

    private const string TemplateResourceName = "My.Talli.Domain..resources.emails.SubscriptionConfirmationEmailNotificationTemplate.html";

    #endregion

    #region <Methods>

    public override SmtpNotification Build(EmailNotificationArgumentOf<SubscriptionConfirmationEmailNotificationPayload> arguments)
    {
        var payload = arguments.Payload;

        var template = Assembly.GetExecutingAssembly().GetManifestResourceContent(TemplateResourceName);

        Body = template
            .Replace("[[Card.LastFour]]", WebUtility.HtmlEncode(payload.CardLastFour))
            .Replace("[[Subscription.Amount]]", WebUtility.HtmlEncode(payload.Amount))
            .Replace("[[Subscription.Plan]]", WebUtility.HtmlEncode(payload.Plan))
            .Replace("[[Subscription.RenewalDate]]", WebUtility.HtmlEncode(payload.RenewalDate))
            .Replace("[[User.FirstName]]", WebUtility.HtmlEncode(payload.FirstName));

        Subject = "You're on Pro! \u2014 MyTalli";

        return FinalizeEmail();
    }

    #endregion
}
