namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class UserAuthenticationApple : DefaultModel
{
	#region <Properties>

	public string AppleId { get; set; } = string.Empty;

	public string DisplayName { get; set; } = string.Empty;

	public string Email { get; set; } = string.Empty;

	public string FirstName { get; set; } = string.Empty;

	public bool IsPrivateRelay { get; set; }

	public string LastName { get; set; } = string.Empty;


	#endregion
}
