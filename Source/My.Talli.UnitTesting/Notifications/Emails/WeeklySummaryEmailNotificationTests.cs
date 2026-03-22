namespace My.Talli.UnitTesting.Notifications.Emails;

using Domain.Notifications.Emails;

/// <summary>Tests</summary>
public class WeeklySummaryEmailNotificationTests
{
	#region <Methods>

	[Fact]
	public void Build_HtmlEncodesPayloadValues()
	{
		var notification = new WeeklySummaryEmailNotification();
		var argument = CreateArgument(firstName: "<em>Robert</em>");

		var result = notification.Build(argument);

		Assert.Contains("&lt;em&gt;Robert&lt;/em&gt;", result.Body);
		Assert.DoesNotContain("<em>Robert</em>", result.Body);
	}

	[Fact]
	public void Build_PlatformRowsNotHtmlEncoded()
	{
		var platformHtml = "<tr><td>Stripe</td><td>$500.00</td></tr>";
		var notification = new WeeklySummaryEmailNotification();
		var argument = CreateArgument(platformRows: platformHtml);

		var result = notification.Build(argument);

		Assert.Contains(platformHtml, result.Body);
	}

	[Fact]
	public void Build_ReplacesAllPlaceholders()
	{
		var notification = new WeeklySummaryEmailNotification();
		var argument = CreateArgument();

		var result = notification.Build(argument);

		Assert.DoesNotContain("[[User.FirstName]]", result.Body);
		Assert.DoesNotContain("[[Summary.TotalRevenue]]", result.Body);
		Assert.DoesNotContain("[[Summary.WeekRange]]", result.Body);
		Assert.DoesNotContain("[[UnsubscribeUrl]]", result.Body);
		Assert.Contains("Robert", result.Body);
		Assert.Contains("$1,234.56", result.Body);
	}

	[Fact]
	public void Build_ReplacesUnsubscribeUrl()
	{
		var notification = new WeeklySummaryEmailNotification();
		var argument = CreateArgument();

		var result = notification.Build(argument);

		Assert.Contains("unsubscribe?token=token-123", result.Body);
	}

	[Fact]
	public void Build_SetsCorrectSubjectWithRevenue()
	{
		var notification = new WeeklySummaryEmailNotification();
		var argument = CreateArgument();

		var result = notification.Build(argument);

		Assert.Contains("Your Week in Review", result.Subject);
		Assert.Contains("$1,234.56", result.Subject);
		Assert.Contains("MyTalli", result.Subject);
	}

	private static EmailNotificationArgumentOf<WeeklySummaryEmailNotificationPayload> CreateArgument(
		string firstName = "Robert",
		string platformRows = "<tr><td>Stripe</td><td>$1,234.56</td></tr>") => new()
	{
		Payload = new WeeklySummaryEmailNotificationPayload
		{
			FirstName = firstName,
			GoalCurrent = "$1,234.56",
			GoalPercent = "62%",
			GoalRemaining = "$765.44",
			GoalTarget = "$2,000.00",
			PlatformRows = platformRows,
			TotalRevenue = "$1,234.56",
			TrendDirection = "up",
			TrendPercent = "12%",
			UnsubscribeToken = "token-123",
			WeekRange = "Mar 15 - Mar 21, 2026",
		}
	};

	#endregion
}
