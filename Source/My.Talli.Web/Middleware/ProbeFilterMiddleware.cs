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
        if (path.Equals("/_blazor/disconnect", StringComparison.OrdinalIgnoreCase)
            && context.Request.Method == HttpMethods.Post)
        {
            context.Response.StatusCode = 200;
            return;
        }

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

        if (path.Contains(".env", StringComparison.OrdinalIgnoreCase)
            || path.Contains(".git", StringComparison.OrdinalIgnoreCase)
            || path.Contains("wp-login", StringComparison.OrdinalIgnoreCase)
            || path.Contains("wp-admin", StringComparison.OrdinalIgnoreCase)
            || path.Contains("wp-includes", StringComparison.OrdinalIgnoreCase)
            || path.Contains("wp-content", StringComparison.OrdinalIgnoreCase)
            || path.Contains("xmlrpc", StringComparison.OrdinalIgnoreCase)
            || path.Contains("phpmy", StringComparison.OrdinalIgnoreCase)
            || path.Contains("phpmyadmin", StringComparison.OrdinalIgnoreCase)
            || path.Contains("admin/config", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".php", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".asp", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".jsp", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".cgi", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = 404;
            return;
        }

        await _next(context);
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
