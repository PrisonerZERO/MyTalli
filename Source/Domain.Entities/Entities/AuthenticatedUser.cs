namespace My.Talli.Domain.Entities;

/// <summary>Keyless entity mapped to auth.vAuthenticatedUser view</summary>
public class AuthenticatedUser
{
	#region <Properties>

	public string DisplayName { get; set; } = string.Empty;

	public string EmailAddress { get; set; } = string.Empty;

	public string FirstName { get; set; } = string.Empty;

	public long Id { get; set; }

	public bool IsDeleted { get; set; }

	public string LastName { get; set; } = string.Empty;

	public string PreferredProvider { get; set; } = string.Empty;


	#endregion
}
