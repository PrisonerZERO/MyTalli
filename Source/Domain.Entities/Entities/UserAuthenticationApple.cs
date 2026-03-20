namespace My.Talli.Domain.Entities;

/// <summary>Entity</summary>
public class UserAuthenticationApple : DefaultEntity
{
	#region <Properties>

	public string AppleId { get; set; } = string.Empty;

	public string DisplayName { get; set; } = string.Empty;

	public string Email { get; set; } = string.Empty;

	public string FirstName { get; set; } = string.Empty;

	public bool IsPrivateRelay { get; set; }

	public string LastName { get; set; } = string.Empty;

	public User User { get; set; } = null!;


	#endregion
}
