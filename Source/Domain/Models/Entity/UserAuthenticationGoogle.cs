namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class UserAuthenticationGoogle : DefaultModel
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


	#endregion
}
