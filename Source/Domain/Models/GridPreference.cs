namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class GridPreference
{
	#region <Properties>

	public string Density { get; set; } = "compact";

	public int PageSize { get; set; } = 50;

	public string SortColumn { get; set; } = "";

	public bool SortDescending { get; set; } = true;

	#endregion
}
