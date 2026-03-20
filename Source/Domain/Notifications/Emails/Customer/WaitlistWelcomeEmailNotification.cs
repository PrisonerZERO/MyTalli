namespace My.Talli.Domain.Notifications.Emails;

using Domain.Extensions;
using System.Net;
using System.Reflection;

/// <summary>Notification</summary>
public class WaitlistWelcomeEmailNotification : EmailNotificationOf<WaitlistWelcomeEmailNotificationPayload>
{
	#region <Variables>

	private const string TemplateResourceName = "My.Talli.Domain..resources.emails.WaitlistWelcomeEmailNotificationTemplate.html";


	#endregion

	#region <Methods>

	public override SmtpNotification Build(EmailNotificationArgumentOf<WaitlistWelcomeEmailNotificationPayload> arguments)
	{
		var payload = arguments.Payload;

		var template = Assembly.GetExecutingAssembly().GetManifestResourceContent(TemplateResourceName);

		Body = template
			.Replace("[[UnsubscribeUrl]]", $"https://www.mytalli.com/unsubscribe?token={WebUtility.UrlEncode(payload.UnsubscribeToken)}")
			.Replace("[[User.FirstName]]", WebUtility.HtmlEncode(payload.FirstName));

		Subject = $"You're on the list, {payload.FirstName}!";

		return FinalizeEmail();
	}


	#endregion
}
