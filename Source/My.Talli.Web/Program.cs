using ElmahCore.Mvc;
using Lamar.Microsoft.DependencyInjection;
using My.Talli.Web.Components;
using My.Talli.Web.Configuration;
using My.Talli.Web.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// -----
// LAMAR
builder.Host.UseLamar();

// --------
// SERVICES
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddRepositories();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddAuthenticationProviders(builder.Configuration);
builder.Services.AddBillingServices(builder.Configuration);
builder.Services.AddEmailServices(builder.Configuration);
builder.Services.AddElmahLogging(builder.Configuration);

// Build
var app = builder.Build();

// ---------------------
// HTTP REQUEST PIPELINE

// BOT/SCANNER PROBE FILTER — short-circuit known vulnerability scan paths
// Returns a bare 404 before the request reaches Elmah, error pages, or Blazor routing
app.Use(async (context, next) =>
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

    await next();
});

app.UseExceptionHandler("/Error", createScopeForErrors: true);
app.UseStatusCodePagesWithReExecute("/Error/{0}");
app.UseElmah();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// WAITLIST MODE — redirect disabled routes to /waitlist
// Remove this block when dashboard features are ready
var disabledRoutes = new[] { "/dashboard", "/suggestions", "/subscription", "/upgrade" };
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.TrimEnd('/') ?? "";
    if (disabledRoutes.Any(r => path.Equals(r, StringComparison.OrdinalIgnoreCase)
        || path.StartsWith(r + "/", StringComparison.OrdinalIgnoreCase)))
    {
        context.Response.Redirect("/waitlist");
        return;
    }
    await next();
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// ---------
// ENDPOINTS
app.MapAuthEndpoints();
app.MapBillingEndpoints();
app.MapEmailEndpoints();

if (app.Environment.IsDevelopment())
    app.MapTestEndpoints();

app.Run();
