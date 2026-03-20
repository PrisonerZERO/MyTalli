namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class AuthenticatedUser
{
	#region <Properties>

	public string DisplayName { get; set; } = string.Empty;

	public string EmailAddress { get; set; } = string.Empty;

	public string FirstName { get; set; } = string.Empty;

	public long Id { get; set; }

	public string InitialProvider { get; set; } = string.Empty;

	public DateTime LastLoginAt { get; set; }

	public string LastName { get; set; } = string.Empty;

	public string PreferredProvider { get; set; } = string.Empty;

	public string? ProviderAvatarUrl { get; set; }

	public bool? ProviderEmailVerified { get; set; }

	public string ProviderId { get; set; } = string.Empty;

	public bool? ProviderIsPrivateRelay { get; set; }

	public string? ProviderLocale { get; set; }

	public string UserPreferences { get; set; } = "{}";


	#endregion
}
