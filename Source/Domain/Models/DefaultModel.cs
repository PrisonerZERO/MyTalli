namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class DefaultModel
{
	#region <Properties>

	public long Id { get; set; }

	public bool IsDeleted { get; set; }

	public bool IsVisible { get; set; } = true;


	#endregion
}
