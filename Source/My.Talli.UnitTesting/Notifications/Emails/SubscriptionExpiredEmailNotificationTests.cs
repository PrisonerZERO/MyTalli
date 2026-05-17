namespace My.Talli.UnitTesting.Notifications.Emails;

using Domain.Notifications.Emails;

/// <summary>Tests</summary>
public class SubscriptionExpiredEmailNotificationTests
{
	#region <Methods>

	[Fact]
	public void Build_ReplacesAllPlaceholders()
	{
		var notification = new SubscriptionExpiredEmailNotification();
		var argument = new EmailNotificationArgumentOf<SubscriptionExpiredEmailNotificationPayload>
		{
			Payload = new SubscriptionExpiredEmailNotificationPayload
			{
				EndDateDisplay = "May 14, 2026",
				FirstName = "Robert",
				PlanName = "Pro Yearly",
				UnsubscribeToken = "token-123"
			}
		};

		var result = notification.Build(argument);

		Assert.Contains("Robert", result.Body);
		Assert.Contains("May 14, 2026", result.Body);
		Assert.Contains("Pro Yearly", result.Body);
		Assert.Contains("unsubscribe?token=token-123", result.Body);
		Assert.DoesNotContain("[[", result.Body);
	}

	[Fact]
	public void Build_HtmlEncodesUserInput()
	{
		var notification = new SubscriptionExpiredEmailNotification();
		var argument = new EmailNotificationArgumentOf<SubscriptionExpiredEmailNotificationPayload>
		{
			Payload = new SubscriptionExpiredEmailNotificationPayload
			{
				EndDateDisplay = "May 14, 2026",
				FirstName = "<script>alert('xss')</script>",
				PlanName = "Pro Monthly",
				UnsubscribeToken = "token-123"
			}
		};

		var result = notification.Build(argument);

		Assert.Contains("&lt;script&gt;", result.Body);
		Assert.DoesNotContain("<script>alert", result.Body);
	}

	[Fact]
	public void Build_EmptyFirstName_FallsBackToThere()
	{
		var notification = new SubscriptionExpiredEmailNotification();
		var argument = new EmailNotificationArgumentOf<SubscriptionExpiredEmailNotificationPayload>
		{
			Payload = new SubscriptionExpiredEmailNotificationPayload
			{
				EndDateDisplay = "May 14, 2026",
				FirstName = string.Empty,
				PlanName = "Pro Monthly",
				UnsubscribeToken = "token-123"
			}
		};

		var result = notification.Build(argument);

		Assert.Contains("Hi there", result.Body);
	}

	[Fact]
	public void Build_SubjectIncludesPlanName()
	{
		var notification = new SubscriptionExpiredEmailNotification();
		var argument = new EmailNotificationArgumentOf<SubscriptionExpiredEmailNotificationPayload>
		{
			Payload = new SubscriptionExpiredEmailNotificationPayload
			{
				EndDateDisplay = "May 14, 2026",
				FirstName = "Robert",
				PlanName = "Pro Yearly",
				UnsubscribeToken = "token-123"
			}
		};

		var result = notification.Build(argument);

		Assert.Equal("Your MyTalli Pro Yearly plan has ended", result.Subject);
	}

	[Fact]
	public void Build_ReturnsNonEmptyHtmlBody()
	{
		var notification = new SubscriptionExpiredEmailNotification();
		var argument = new EmailNotificationArgumentOf<SubscriptionExpiredEmailNotificationPayload>
		{
			Payload = new SubscriptionExpiredEmailNotificationPayload
			{
				EndDateDisplay = "May 14, 2026",
				FirstName = "Robert",
				PlanName = "Pro Yearly",
				UnsubscribeToken = "token-123"
			}
		};

		var result = notification.Build(argument);

		Assert.False(string.IsNullOrWhiteSpace(result.Body));
		Assert.True(result.IsBodyHtml);
	}

	#endregion
}
