namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class User
{
	#region <Properties>

	public string DisplayName { get; set; } = string.Empty;

	public string FirstName { get; set; } = string.Empty;

	public long Id { get; set; }

	public string InitialProvider { get; set; } = string.Empty;

	public DateTime LastLoginAt { get; set; }

	public string LastName { get; set; } = string.Empty;

	public string PreferredProvider { get; set; } = string.Empty;

	public string UserPreferences { get; set; } = "{}";

	#endregion
}
