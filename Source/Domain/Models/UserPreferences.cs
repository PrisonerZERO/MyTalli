namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class UserPreferences
{
	#region <Properties>

	public string DarkMode { get; set; } = "system";

	public EmailPreferences EmailPreferences { get; set; } = new();

	public bool FunGreetings { get; set; } = true;

	public Dictionary<string, GridPreference> GridPreferences { get; set; } = new();

	#endregion
}
