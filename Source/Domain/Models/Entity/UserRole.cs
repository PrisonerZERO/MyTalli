namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class UserRole
{
	#region <Properties>

	public long Id { get; set; }

	public string Role { get; set; } = string.Empty;

	public long UserId { get; set; }

	#endregion
}
