namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class UserRole : DefaultModel
{
	#region <Properties>

	public string Role { get; set; } = string.Empty;

	public long UserId { get; set; }

	#endregion
}
