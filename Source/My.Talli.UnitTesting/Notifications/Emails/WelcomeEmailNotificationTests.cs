namespace My.Talli.UnitTesting.Notifications.Emails;

using Domain.Notifications.Emails;

/// <summary>Tests</summary>
public class WelcomeEmailNotificationTests
{
	#region <Methods>

	[Fact]
	public void Build_HtmlEncodesUserInput()
	{
		var notification = new WelcomeEmailNotification();
		var argument = new EmailNotificationArgumentOf<WelcomeEmailNotificationPayload>
		{
			Payload = new WelcomeEmailNotificationPayload
			{
				FirstName = "<script>alert('xss')</script>",
				UnsubscribeToken = "token-123",
			}
		};

		var result = notification.Build(argument);

		Assert.Contains("&lt;script&gt;", result.Body);
		Assert.DoesNotContain("<script>alert", result.Body);
	}

	[Fact]
	public void Build_ReplacesFirstNamePlaceholder()
	{
		var notification = new WelcomeEmailNotification();
		var argument = new EmailNotificationArgumentOf<WelcomeEmailNotificationPayload>
		{
			Payload = new WelcomeEmailNotificationPayload
			{
				FirstName = "Robert",
				UnsubscribeToken = "token-123",
			}
		};

		var result = notification.Build(argument);

		Assert.Contains("Robert", result.Body);
		Assert.DoesNotContain("[[User.FirstName]]", result.Body);
	}

	[Fact]
	public void Build_ReplacesUnsubscribeUrlPlaceholder()
	{
		var notification = new WelcomeEmailNotification();
		var argument = new EmailNotificationArgumentOf<WelcomeEmailNotificationPayload>
		{
			Payload = new WelcomeEmailNotificationPayload
			{
				FirstName = "Robert",
				UnsubscribeToken = "abc123",
			}
		};

		var result = notification.Build(argument);

		Assert.Contains("unsubscribe?token=abc123", result.Body);
		Assert.DoesNotContain("[[UnsubscribeUrl]]", result.Body);
	}

	[Fact]
	public void Build_ReturnsNonEmptyHtmlBody()
	{
		var notification = new WelcomeEmailNotification();
		var argument = new EmailNotificationArgumentOf<WelcomeEmailNotificationPayload>
		{
			Payload = new WelcomeEmailNotificationPayload
			{
				FirstName = "Robert",
				UnsubscribeToken = "token-123",
			}
		};

		var result = notification.Build(argument);

		Assert.False(string.IsNullOrWhiteSpace(result.Body));
		Assert.True(result.IsBodyHtml);
	}

	[Fact]
	public void Build_SetsCorrectSubject()
	{
		var notification = new WelcomeEmailNotification();
		var argument = new EmailNotificationArgumentOf<WelcomeEmailNotificationPayload>
		{
			Payload = new WelcomeEmailNotificationPayload
			{
				FirstName = "Robert",
				UnsubscribeToken = "token-123",
			}
		};

		var result = notification.Build(argument);

		Assert.Equal("Welcome to MyTalli, Robert!", result.Subject);
	}

	#endregion
}
