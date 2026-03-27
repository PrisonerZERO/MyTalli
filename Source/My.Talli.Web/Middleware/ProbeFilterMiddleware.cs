namespace My.Talli.Web.Middleware;

/// <summary>Middleware</summary>
public class ProbeFilterMiddleware
{
    #region <Variables>

    private readonly RequestDelegate _next;

    #endregion

    #region <Constructors>

    public ProbeFilterMiddleware(RequestDelegate next)
    {
        _next = next;
    }


    #endregion

    #region <Methods>

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";

        // BLAZOR DISCONNECT — Expired Circuits return 400, polluting Elmah. Short-circuit with 200.
        if (path.Equals("/_blazor/disconnect", StringComparison.OrdinalIgnoreCase) && context.Request.Method == HttpMethods.Post)
        {
            context.Response.StatusCode = 200;
            return;
        }

        // HEAD — Blazor doesn't handle HEAD requests natively (returns 405). Rewrite to GET so the pipeline processes it normally; Kestrel automatically strips the response body for HEAD.
        if (context.Request.Method == HttpMethods.Head)
            context.Request.Method = HttpMethods.Get;

        // OPTIONS — Is used by "Office link probes (Word, Outlook)" and CORS preflights.  Short-circuit with 200.
        if (context.Request.Method == HttpMethods.Options)
        {
            context.Response.StatusCode = 204;
            return;
        }

        // AZURE HEALTH PROBE — App Service liveness check requests /robots933456.txt. Short-circuit with 200.
        if (path.Equals("/robots933456.txt", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = 200;
            return;
        }

        // TRAILING DOT — Users copy URLs from sentences ("visit mytalli.com/signin.") and the trailing period becomes part of the path. Redirect to the clean URL.
        if (path.Length > 1 && path.EndsWith('.'))
        {
            var cleanPath = path.TrimEnd('.');
            var query = context.Request.QueryString.Value ?? "";

            context.Response.StatusCode = 301;
            context.Response.Headers.Location = cleanPath + query;
            return;
        }

        // BOT/SCANNER PROBES — Known attack paths, should short-circuit with 404.
        if (IsProbeRequest(path))
        {
            context.Response.StatusCode = 404;
            return;
        }

        await _next(context);
    }

    private static bool IsProbeRequest(string path)
    {
        return path.Contains(".env", StringComparison.OrdinalIgnoreCase)
            || path.Contains(".git", StringComparison.OrdinalIgnoreCase)
            || path.Contains("wp-", StringComparison.OrdinalIgnoreCase)
            || path.Contains("xmlrpc", StringComparison.OrdinalIgnoreCase)
            || path.Contains("phpmy", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".php", StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}

/// <summary>Extension</summary>
public static class ProbeFilterMiddlewareExtensions
{
    #region <Methods>

    public static IApplicationBuilder UseProbeFilter(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ProbeFilterMiddleware>();
    }


    #endregion
}
