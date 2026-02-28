using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using My.Talli.Web.Components;
using My.Talli.Web.Services.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseLamar();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// --------------
// AUTHENTICATION
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/signin";
        options.LogoutPath = "/api/auth/logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    })
    .AddGoogle(options =>
    {
        // Configuration Uses: dotnet user-secrets
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
        options.CallbackPath = "/signin-google";
        options.Events.OnCreatingTicket = async context =>
        {
            var handler = context.HttpContext.RequestServices.GetRequiredService<GoogleAuthenticationHandler>();
            await handler.HandleTicketAsync(context);
        };
    });

builder.Services.AddScoped<GoogleAuthenticationHandler>();

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// Build
var app = builder.Build();

// ---------------------
// HTTP REQUEST PIPELINE
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// ---------
// ENDPOINTS
app.MapGet("/api/auth/login/{provider}", async (string provider, HttpContext context) =>
{
    var properties = new AuthenticationProperties
    {
        RedirectUri = "/waitlist"
    };

    var scheme = provider.ToLowerInvariant() switch
    {
        "google" => GoogleDefaults.AuthenticationScheme,
        _ => throw new ArgumentException($"Unsupported provider: {provider}")
    };

    await context.ChallengeAsync(scheme, properties);
});

app.MapGet("/api/auth/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    context.Response.Redirect("/");
});

app.Run();