namespace My.Talli.Web.Configuration;

using Web.Services.Export;

/// <summary>Configuration</summary>
public static class ExportConfiguration
{
	#region <Methods>

	public static void AddExportServices(this IServiceCollection services)
	{
		services.AddScoped<CsvExportService>();
	}


	#endregion
}
