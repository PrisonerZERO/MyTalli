namespace My.Talli.Web.Middleware;

using Domain.Data.Interfaces;
using System.Security.Claims;

/// <summary>Middleware</summary>
public class CurrentUserMiddleware
{
	#region <Variables>

	private readonly RequestDelegate _next;

	#endregion

	#region <Constructors>

	public CurrentUserMiddleware(RequestDelegate next)
	{
		_next = next;
	}

	#endregion

	#region <Methods>

	public async Task InvokeAsync(HttpContext context, ICurrentUserService currentUserService, ILogger<CurrentUserMiddleware> logger)
	{
		if (context.User.Identity?.IsAuthenticated == true)
		{
			var userIdClaim = context.User.FindFirst("UserId")?.Value;
			var displayName = context.User.FindFirst("DisplayName")?.Value
				?? context.User.FindFirst(ClaimTypes.Name)?.Value
				?? string.Empty;

			if (long.TryParse(userIdClaim, out var userId))
			{
				currentUserService.Set(userId, displayName);
				logger.LogDebug("CurrentUserMiddleware: Set UserId={UserId} for {Path}", userId, context.Request.Path);
			}
			else
			{
				logger.LogWarning("CurrentUserMiddleware: UserId claim not found or not parseable for {Path}. Claims: {Claims}",
					context.Request.Path,
					string.Join(", ", context.User.Claims.Select(c => $"{c.Type}={c.Value}")));
			}
		}
		else if (context.Request.Cookies.ContainsKey("talli-theme"))
		{
			// Theme preference belongs to authenticated users; stale cookie from a prior session must not survive.
			context.Response.Cookies.Delete("talli-theme", new CookieOptions { Path = "/" });
		}

		await _next(context);
	}

	#endregion
}

/// <summary>Extension</summary>
public static class CurrentUserMiddlewareExtensions
{
	#region <Methods>

	public static IApplicationBuilder UseCurrentUser(this IApplicationBuilder app)
	{
		return app.UseMiddleware<CurrentUserMiddleware>();
	}

	#endregion
}
