namespace My.Talli.Domain.Entities;

/// <summary>Entity</summary>
public class UserAuthenticationGoogle : DefaultEntity
{
	#region <Properties>

	public string AvatarUrl { get; set; } = string.Empty;

	public string DisplayName { get; set; } = string.Empty;

	public string Email { get; set; } = string.Empty;

	public bool EmailVerified { get; set; }

	public string FirstName { get; set; } = string.Empty;

	public string GoogleId { get; set; } = string.Empty;

	public string LastName { get; set; } = string.Empty;

	public string Locale { get; set; } = string.Empty;

	public User User { get; set; } = null!;

	#endregion
}
