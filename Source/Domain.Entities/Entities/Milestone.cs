namespace My.Talli.Domain.Entities;

/// <summary>Entity</summary>
public class Milestone : DefaultEntity
{
	#region <Properties>

	public string Description { get; set; } = string.Empty;

	public string MilestoneGroup { get; set; } = string.Empty;

	public int SortOrder { get; set; }

	public string Status { get; set; } = string.Empty;

	public string Title { get; set; } = string.Empty;


	#endregion
}
