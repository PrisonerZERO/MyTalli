namespace My.Talli.UnitTesting.Components.JsonSerializers;

using Domain.Components.JsonSerializers;
using Domain.Models;

/// <summary>Tests</summary>
public class UserPreferencesJsonSerializerTests
{
	#region <Variables>

	private readonly UserPreferencesJsonSerializer _serializer = new();

	#endregion

	#region <Methods>

	[Fact]
	public void Deserialize_EmptyInput_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => _serializer.Deserialize(""));
	}

	[Fact]
	public void Deserialize_NullInput_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => _serializer.Deserialize(null!));
	}

	[Fact]
	public void Deserialize_ValidJson_ReturnsCorrectValues()
	{
		var json = "{\"emailPreferences\":{\"subscriptionConfirmationEmail\":false,\"unsubscribeAll\":true,\"weeklySummaryEmail\":false},\"funGreetings\":false}";

		var result = _serializer.Deserialize(json);

		Assert.True(result.EmailPreferences.UnsubscribeAll);
		Assert.False(result.EmailPreferences.SubscriptionConfirmationEmail);
		Assert.False(result.EmailPreferences.WeeklySummaryEmail);
		Assert.False(result.FunGreetings);
	}

	[Fact]
	public void Deserialize_WhitespaceInput_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => _serializer.Deserialize("   "));
	}

	[Fact]
	public void RoundTrip_SerializeDeserialize_PreservesAllValues()
	{
		var original = new UserPreferences
		{
			FunGreetings = false,
			EmailPreferences = new EmailPreferences
			{
				UnsubscribeAll = true,
				SubscriptionConfirmationEmail = false,
				WeeklySummaryEmail = false,
			}
		};

		var json = _serializer.Serialize(original);
		var result = _serializer.Deserialize(json);

		Assert.Equal(original.FunGreetings, result.FunGreetings);
		Assert.Equal(original.EmailPreferences.UnsubscribeAll, result.EmailPreferences.UnsubscribeAll);
		Assert.Equal(original.EmailPreferences.SubscriptionConfirmationEmail, result.EmailPreferences.SubscriptionConfirmationEmail);
		Assert.Equal(original.EmailPreferences.WeeklySummaryEmail, result.EmailPreferences.WeeklySummaryEmail);
	}

	[Fact]
	public void RoundTrip_GridPreferences_PreservesAllValues()
	{
		var original = new UserPreferences();
		original.GridPreferences["manualEntry.entryGrid"] = new GridPreference
		{
			Density = "compact",
			PageSize = 25,
			SortColumn = "Description",
			SortDescending = false,
		};

		var json = _serializer.Serialize(original);
		var result = _serializer.Deserialize(json);

		Assert.True(result.GridPreferences.ContainsKey("manualEntry.entryGrid"));
		var grid = result.GridPreferences["manualEntry.entryGrid"];
		Assert.Equal("compact", grid.Density);
		Assert.Equal(25, grid.PageSize);
		Assert.Equal("Description", grid.SortColumn);
		Assert.False(grid.SortDescending);
	}

	[Fact]
	public void Deserialize_MissingGridPreferences_ReturnsEmptyDictionary()
	{
		var json = "{\"emailPreferences\":{},\"funGreetings\":true}";

		var result = _serializer.Deserialize(json);

		Assert.NotNull(result.GridPreferences);
		Assert.Empty(result.GridPreferences);
	}

	[Fact]
	public void Serialize_CustomValues_PreservesValues()
	{
		var preferences = new UserPreferences
		{
			FunGreetings = false,
			EmailPreferences = new EmailPreferences { UnsubscribeAll = true, WeeklySummaryEmail = false }
		};

		var json = _serializer.Serialize(preferences);

		Assert.Contains("\"unsubscribeAll\":true", json);
		Assert.Contains("\"weeklySummaryEmail\":false", json);
		Assert.Contains("\"funGreetings\":false", json);
	}

	[Fact]
	public void Serialize_DefaultPreferences_ReturnsCamelCaseJson()
	{
		var preferences = new UserPreferences();

		var json = _serializer.Serialize(preferences);

		Assert.Contains("\"emailPreferences\"", json);
		Assert.Contains("\"funGreetings\"", json);
		Assert.DoesNotContain("\"EmailPreferences\"", json);
		Assert.DoesNotContain("\"FunGreetings\"", json);
	}

	[Fact]
	public void Serialize_NullPreferences_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => _serializer.Serialize(null!));
	}

	#endregion
}
