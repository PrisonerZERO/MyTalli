namespace My.Talli.Web.Queries;

/// <summary>Args</summary>
public class PageArgs
{
	#region <Properties>

	public int PageNumber { get; init; } = 1;

	public int PageSize { get; init; } = 50;

	public int Skip => Math.Max(0, (PageNumber - 1) * PageSize);

	public int Take => Math.Max(1, PageSize);

	#endregion
}
