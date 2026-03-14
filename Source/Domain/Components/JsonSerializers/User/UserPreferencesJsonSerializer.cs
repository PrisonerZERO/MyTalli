namespace My.Talli.Domain.Components.JsonSerializers;

using System.Text.Json;
using Domain.Framework;

using MODELS = Domain.Models;

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

	public MODELS.UserPreferences Deserialize(string json)
	{
		Assert.IsNullOrWhitespace(json, nameof(json));

		return JsonSerializer.Deserialize<MODELS.UserPreferences>(json, Options) ?? new MODELS.UserPreferences();
	}

	public string Serialize(MODELS.UserPreferences preferences)
	{
		Assert.IsNotNull(preferences, nameof(preferences));

		return JsonSerializer.Serialize(preferences, Options);
	}

	#endregion
}
