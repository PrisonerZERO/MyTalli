namespace My.Talli.Domain.Notifications.Emails;

using Domain.Extensions;
using System.Net;
using System.Reflection;

/// <summary>Notification</summary>
public class SubscriptionExpiredEmailNotification : EmailNotificationOf<SubscriptionExpiredEmailNotificationPayload>
{
	#region <Variables>

	private const string TemplateResourceName = "My.Talli.Domain..resources.emails.SubscriptionExpiredEmailNotificationTemplate.html";

	#endregion

	#region <Methods>

	public override SmtpNotification Build(EmailNotificationArgumentOf<SubscriptionExpiredEmailNotificationPayload> arguments)
	{
		var payload = arguments.Payload;
		var firstName = string.IsNullOrWhiteSpace(payload.FirstName) ? "there" : payload.FirstName;
		var planName = string.IsNullOrWhiteSpace(payload.PlanName) ? "Pro" : payload.PlanName;

		var template = Assembly.GetExecutingAssembly().GetManifestResourceContent(TemplateResourceName);

		Body = template
			.Replace("[[EndDateDisplay]]", WebUtility.HtmlEncode(payload.EndDateDisplay))
			.Replace("[[PlanName]]", WebUtility.HtmlEncode(planName))
			.Replace("[[UnsubscribeUrl]]", $"https://www.mytalli.com/unsubscribe?token={WebUtility.UrlEncode(payload.UnsubscribeToken)}")
			.Replace("[[User.FirstName]]", WebUtility.HtmlEncode(firstName));

		Subject = $"Your MyTalli {planName} plan has ended";

		return FinalizeEmail();
	}

	#endregion
}
