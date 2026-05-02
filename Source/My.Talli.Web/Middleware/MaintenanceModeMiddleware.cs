namespace My.Talli.Web.Middleware;

using Web.Services.Admin;

/// <summary>Middleware</summary>
public class MaintenanceModeMiddleware
{
	#region <Constants>

	private const string MaintenancePath = "/maintenance";

	private static readonly string[] WhitelistPrefixes =
	[
		"/maintenance",
		"/api/admin/maintenance",
		"/_blazor",
		"/_framework",
		"/css",
		"/js",
		"/lib",
		"/Error"
	];


	#endregion

	#region <Variables>

	private readonly RequestDelegate _next;


	#endregion

	#region <Constructors>

	public MaintenanceModeMiddleware(RequestDelegate next)
	{
		_next = next;
	}


	#endregion

	#region <Methods>

	public async Task InvokeAsync(HttpContext context, IMaintenanceModeService maintenanceModeService)
	{
		if (!maintenanceModeService.IsEnabled)
		{
			await _next(context);
			return;
		}

		if (context.User.IsInRole("Admin"))
		{
			await _next(context);
			return;
		}

		var path = context.Request.Path.Value ?? string.Empty;

		if (IsWhitelisted(path))
		{
			await _next(context);
			return;
		}

		context.Response.Redirect(MaintenancePath);
	}

	private static bool IsWhitelisted(string path)
	{
		foreach (var prefix in WhitelistPrefixes)
		{
			if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
				return true;
		}

		return false;
	}


	#endregion
}

/// <summary>Extension</summary>
public static class MaintenanceModeMiddlewareExtensions
{
	#region <Methods>

	public static IApplicationBuilder UseMaintenanceMode(this IApplicationBuilder app)
	{
		return app.UseMiddleware<MaintenanceModeMiddleware>();
	}


	#endregion
}
