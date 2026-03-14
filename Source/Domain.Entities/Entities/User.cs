namespace My.Talli.Domain.Entities.Entities;

/// <summary>Entity</summary>
public class User : DefaultEntity
{
	#region <Properties>

	public string DisplayName { get; set; } = string.Empty;

	public string FirstName { get; set; } = string.Empty;

	public string InitialProvider { get; set; } = string.Empty;

	public DateTime LastLoginAt { get; set; }

	public string LastName { get; set; } = string.Empty;

	public string PreferredProvider { get; set; } = string.Empty;

	public string UserPreferences { get; set; } = "{}";

	#endregion
}
