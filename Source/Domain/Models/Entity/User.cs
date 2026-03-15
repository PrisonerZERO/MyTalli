namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class User : DefaultModel
{
	#region <Properties>

	public string DisplayName { get; set; } = string.Empty;

	public string FirstName { get; set; } = string.Empty;

	public string InitialProvider { get; set; } = string.Empty;

	public bool IsNewUser { get; set; }

	public DateTime LastLoginAt { get; set; }

	public string LastName { get; set; } = string.Empty;

	public string PreferredProvider { get; set; } = string.Empty;

	public List<string> Roles { get; set; } = [];

	public string UserPreferences { get; set; } = "{}";

	#endregion
}
