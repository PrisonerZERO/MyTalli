using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using AspNet.Security.OAuth.Apple;
using My.Talli.Web.Components;
using My.Talli.Web.Services.Authentication;
using AppleAuthHandler = My.Talli.Web.Services.Authentication.AppleAuthenticationHandler;

var builder = WebApplication.CreateBuilder(args);

// -----
// LAMAR
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
    })
    .AddMicrosoftAccount(options =>
    {
        // Configuration Uses: dotnet user-secrets
        options.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"]!;
        options.CallbackPath = "/signin-microsoft";
        options.Events.OnCreatingTicket = async context =>
        {
            var handler = context.HttpContext.RequestServices.GetRequiredService<MicrosoftAuthenticationHandler>();
            await handler.HandleTicketAsync(context);
        };
    })
    .AddApple(options =>
    {
        // Configuration Uses: dotnet user-secrets
        options.ClientId = builder.Configuration["Authentication:Apple:ClientId"]!;
        options.TeamId = builder.Configuration["Authentication:Apple:TeamId"]!;
        options.KeyId = builder.Configuration["Authentication:Apple:KeyId"]!;
        options.GenerateClientSecret = true;
        options.CallbackPath = "/signin-apple";
        options.PrivateKey = async (keyId, cancellationToken) =>
        {
            var keyPath = builder.Configuration["Authentication:Apple:PrivateKeyPath"]!;
            var keyText = await File.ReadAllTextAsync(keyPath, cancellationToken);
            return keyText.AsMemory();
        };
        options.Events.OnCreatingTicket = async context =>
        {
            var handler = context.HttpContext.RequestServices.GetRequiredService<AppleAuthHandler>();
            await handler.HandleTicketAsync(context);
        };
    });

builder.Services.AddScoped<AppleAuthHandler>();
builder.Services.AddScoped<GoogleAuthenticationHandler>();
builder.Services.AddScoped<MicrosoftAuthenticationHandler>();

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
        "apple" => AppleAuthenticationDefaults.AuthenticationScheme,
        "google" => GoogleDefaults.AuthenticationScheme,
        "microsoft" => MicrosoftAccountDefaults.AuthenticationScheme,
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