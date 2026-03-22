namespace My.Talli.UnitTesting.Models;

using Domain.Models;

/// <summary>Tests</summary>
public class EmailPreferencesTests
{
	#region <Methods>

	[Fact]
	public void NewInstance_SubscriptionConfirmationEmail_DefaultsToTrue()
	{
		var preferences = new EmailPreferences();

		Assert.True(preferences.SubscriptionConfirmationEmail);
	}

	[Fact]
	public void NewInstance_UnsubscribeAll_DefaultsToFalse()
	{
		var preferences = new EmailPreferences();

		Assert.False(preferences.UnsubscribeAll);
	}

	[Fact]
	public void NewInstance_WeeklySummaryEmail_DefaultsToTrue()
	{
		var preferences = new EmailPreferences();

		Assert.True(preferences.WeeklySummaryEmail);
	}

	#endregion
}
