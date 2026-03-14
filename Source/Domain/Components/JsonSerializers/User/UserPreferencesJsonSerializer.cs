namespace My.Talli.Domain.Components.JsonSerializers;

using System.Text.Json;
using My.Talli.Domain.Framework;
using Models = My.Talli.Domain.Models;

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

	public Models.UserPreferences Deserialize(string json)
	{
		Assert.IsNullOrWhitespace(json, nameof(json));

		return JsonSerializer.Deserialize<Models.UserPreferences>(json, Options) ?? new Models.UserPreferences();
	}

	public string Serialize(Models.UserPreferences preferences)
	{
		Assert.IsNotNull(preferences, nameof(preferences));

		return JsonSerializer.Serialize(preferences, Options);
	}

	#endregion
}
