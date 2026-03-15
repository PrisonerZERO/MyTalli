namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class UserAuthenticationMicrosoft
{
	#region <Properties>

	public string DisplayName { get; set; } = string.Empty;

	public string Email { get; set; } = string.Empty;

	public string FirstName { get; set; } = string.Empty;

	public long Id { get; set; }

	public string LastName { get; set; } = string.Empty;

	public string MicrosoftId { get; set; } = string.Empty;

	public long UserId { get; set; }

	#endregion
}
