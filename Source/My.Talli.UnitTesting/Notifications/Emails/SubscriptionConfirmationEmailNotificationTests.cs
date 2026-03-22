namespace My.Talli.UnitTesting.Notifications.Emails;

using Domain.Notifications.Emails;

/// <summary>Tests</summary>
public class SubscriptionConfirmationEmailNotificationTests
{
	#region <Methods>

	[Fact]
	public void Build_HtmlEncodesPayloadValues()
	{
		var notification = new SubscriptionConfirmationEmailNotification();
		var argument = CreateArgument(firstName: "<b>Robert</b>");

		var result = notification.Build(argument);

		Assert.Contains("&lt;b&gt;Robert&lt;/b&gt;", result.Body);
		Assert.DoesNotContain("<b>Robert</b>", result.Body);
	}

	[Fact]
	public void Build_ReplacesAllPlaceholders()
	{
		var notification = new SubscriptionConfirmationEmailNotification();
		var argument = CreateArgument();

		var result = notification.Build(argument);

		Assert.DoesNotContain("[[", result.Body);
		Assert.Contains("Robert", result.Body);
		Assert.Contains("$12.00", result.Body);
		Assert.Contains("Monthly Pro", result.Body);
		Assert.Contains("April 22, 2026", result.Body);
		Assert.Contains("4242", result.Body);
	}

	[Fact]
	public void Build_ReplacesUnsubscribeUrl()
	{
		var notification = new SubscriptionConfirmationEmailNotification();
		var argument = CreateArgument();

		var result = notification.Build(argument);

		Assert.Contains("unsubscribe?token=token-123", result.Body);
	}

	[Fact]
	public void Build_SetsCorrectSubject()
	{
		var notification = new SubscriptionConfirmationEmailNotification();
		var argument = CreateArgument();

		var result = notification.Build(argument);

		Assert.Contains("You're on Pro!", result.Subject);
		Assert.Contains("MyTalli", result.Subject);
	}

	private static EmailNotificationArgumentOf<SubscriptionConfirmationEmailNotificationPayload> CreateArgument(
		string firstName = "Robert") => new()
	{
		Payload = new SubscriptionConfirmationEmailNotificationPayload
		{
			Amount = "$12.00",
			CardLastFour = "4242",
			FirstName = firstName,
			Plan = "Monthly Pro",
			RenewalDate = "April 22, 2026",
			UnsubscribeToken = "token-123",
		}
	};

	#endregion
}
