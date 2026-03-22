namespace My.Talli.Domain.Entities;

/// <summary>Entity</summary>
public class Suggestion : DefaultEntity
{
	#region <Properties>

	public string Category { get; set; } = string.Empty;

	public string Description { get; set; } = string.Empty;

	public string Status { get; set; } = string.Empty;

	public string Title { get; set; } = string.Empty;

	public User User { get; set; } = null!;

	public long UserId { get; set; }


	#endregion
}
