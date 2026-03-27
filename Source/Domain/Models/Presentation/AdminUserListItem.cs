namespace My.Talli.Domain.Models;

/// <summary>Presentation model for admin user list</summary>
public class AdminUserListItem
{
	#region <Properties>

	public string DisplayName { get; set; } = string.Empty;

	public string Email { get; set; } = string.Empty;

	public string FirstName { get; set; } = string.Empty;

	public bool HasActiveSubscription { get; set; }

	public string PreferredProvider { get; set; } = string.Empty;

	public long UserId { get; set; }


	#endregion
}
