using ElmahCore.Mvc;
using Lamar.Microsoft.DependencyInjection;
using My.Talli.Domain.DI.Lamar.IoC;
using My.Talli.Web.Components;
using My.Talli.Web.Configuration;
using My.Talli.Web.Endpoints;
using My.Talli.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

// -----
// LAMAR
builder.Host.UseLamar(services => services.IncludeRegistry<ContainerRegistry>());

// --------
// SERVICES
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddRepositories();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddAuthenticationProviders(builder.Configuration);
builder.Services.AddBillingServices(builder.Configuration);
builder.Services.AddEmailServices(builder.Configuration);
builder.Services.AddPlatformServices(builder.Configuration);
builder.Services.AddAdminServices();
builder.Services.AddExportServices();
builder.Services.AddKnownIssue(builder.Configuration);
builder.Services.AddElmahLogging(builder.Configuration);

// Build
var app = builder.Build();

// ---------------------
// HTTP REQUEST PIPELINE

// BOT/SCANNER PROBE FILTER — short-circuit known vulnerability scan paths
// Returns a bare 404 before the request reaches Elmah, error pages, or Blazor routing
app.UseProbeFilter();

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
app.UseCurrentUser();
app.UseMaintenanceMode();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// ---------
// ENDPOINTS
app.MapAdminEndpoints();
app.MapAuthEndpoints();
app.MapBillingEndpoints();
app.MapEmailEndpoints();
app.MapExportEndpoints();
app.MapPlatformEndpoints();

if (app.Environment.IsDevelopment())
    app.MapTestEndpoints();

app.Run();