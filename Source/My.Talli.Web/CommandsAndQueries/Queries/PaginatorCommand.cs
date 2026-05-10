namespace My.Talli.Web.Queries;

/// <summary>Command</summary>
public static class PaginatorCommand
{
	#region <Methods>

	public static IQueryable<T> Query<T>(IQueryable<T> source, PageArgs pageArgs)
	{
		return source.Skip(pageArgs.Skip).Take(pageArgs.Take);
	}

	#endregion
}
