namespace My.Talli.Web.Configuration;

using Web.Queries;

/// <summary>Configuration</summary>
public static class QueriesConfiguration
{
	#region <Methods>

	public static void AddQueries(this IServiceCollection services)
	{
		services.AddScoped<ExpenseFindCommand>();
		services.AddScoped<ManualEntryFindCommand>();
		services.AddScoped<PayoutFindCommand>();
		services.AddScoped<RevenueFindCommand>();
	}

	#endregion
}
