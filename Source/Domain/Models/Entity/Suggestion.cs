namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class Suggestion : DefaultModel
{
	#region <Properties>

	public string? AdminNote { get; set; }

	public string Category { get; set; } = string.Empty;

	public DateTime CreatedOn { get; set; }

	public string Description { get; set; } = string.Empty;

	public string Status { get; set; } = string.Empty;

	public string Title { get; set; } = string.Empty;

	public long UserId { get; set; }


	#endregion
}
