namespace My.Talli.Domain.Entities;

/// <summary>Entity</summary>
public class UserRole : DefaultEntity
{
	#region <Properties>

	public string Role { get; set; } = string.Empty;

	public User User { get; set; } = null!;

	public long UserId { get; set; }

	#endregion
}
