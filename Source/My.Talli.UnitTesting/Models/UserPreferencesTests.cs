namespace My.Talli.UnitTesting.Models;

using Domain.Models;

/// <summary>Tests</summary>
public class UserPreferencesTests
{
	#region <Methods>

	[Fact]
	public void NewInstance_EmailPreferences_IsNotNull()
	{
		var preferences = new UserPreferences();

		Assert.NotNull(preferences.EmailPreferences);
	}

	[Fact]
	public void NewInstance_FunGreetings_DefaultsToTrue()
	{
		var preferences = new UserPreferences();

		Assert.True(preferences.FunGreetings);
	}

	#endregion
}
