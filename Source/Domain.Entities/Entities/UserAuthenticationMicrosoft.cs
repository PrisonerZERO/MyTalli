namespace My.Talli.Domain.Entities;

/// <summary>Entity</summary>
public class UserAuthenticationMicrosoft : DefaultEntity
{
	#region <Properties>

	public string DisplayName { get; set; } = string.Empty;

	public string Email { get; set; } = string.Empty;

	public string FirstName { get; set; } = string.Empty;

	public string LastName { get; set; } = string.Empty;

	public string MicrosoftId { get; set; } = string.Empty;

	public User User { get; set; } = null!;


	#endregion
}
