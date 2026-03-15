namespace My.Talli.Domain.Components.JsonSerializers;

using System.Text.Json;
using Domain.Framework;
using Domain.Models;

/// <summary>Component</summary>
public class UserPreferencesJsonSerializer
{
	#region <Variables>

	private static readonly JsonSerializerOptions Options = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false
	};

	#endregion

	#region <Methods>

	public UserPreferences Deserialize(string json)
	{
		Assert.IsNullOrWhitespace(json, nameof(json));

		return JsonSerializer.Deserialize<UserPreferences>(json, Options) ?? new UserPreferences();
	}

	public string Serialize(UserPreferences preferences)
	{
		Assert.IsNotNull(preferences, nameof(preferences));

		return JsonSerializer.Serialize(preferences, Options);
	}

	#endregion
}
