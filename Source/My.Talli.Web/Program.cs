using ElmahCore.Mvc;
using ElmahCore.Sql;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using My.Talli.Domain.Components.JsonSerializers;
using My.Talli.Domain.Components.Tokens;
using My.Talli.Domain.Data.EntityFramework;
using My.Talli.Domain.Data.EntityFramework.Repositories;
using My.Talli.Domain.Data.EntityFramework.Resolvers;
using My.Talli.Domain.Data.Interfaces;
using My.Talli.Domain.Handlers.Authentication;
using My.Talli.Domain.Mappers;
using My.Talli.Domain.Repositories;
using My.Talli.Web.Components;
using My.Talli.Web.Endpoints;
using My.Talli.Web.Services.Authentication;
using My.Talli.Web.Services.Billing;
using My.Talli.Web.Services.Email;
using My.Talli.Web.Services.Identity;
using My.Talli.Web.Services.Tokens;

using APPLE_AUTH_HANDLER = My.Talli.Web.Services.Authentication.AppleAuthenticationHandler;
using ENTITIES = My.Talli.Domain.Entities;
using MODELS = My.Talli.Domain.Models;

var builder = WebApplication.CreateBuilder(args);

// -----
// LAMAR
builder.Host.UseLamar();

// --------
// DATABASE
builder.Services.AddDbContext<TalliDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ------------
// REPOSITORIES
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped(typeof(IAuditResolver<>), typeof(AuditResolver<>));
builder.Services.AddScoped(typeof(IAuditableRepositoryAsync<>), typeof(GenericAuditableRepositoryAsync<>));
builder.Services.AddScoped(typeof(RepositoryAdapterAsync<,>));
builder.Services.AddScoped<IEntityMapper<MODELS.Billing, ENTITIES.Billing>, BillingMapper>();
builder.Services.AddScoped<IEntityMapper<MODELS.BillingStripe, ENTITIES.BillingStripe>, BillingStripeMapper>();
builder.Services.AddScoped<IEntityMapper<MODELS.Order, ENTITIES.Order>, OrderMapper>();
builder.Services.AddScoped<IEntityMapper<MODELS.OrderItem, ENTITIES.OrderItem>, OrderItemMapper>();
builder.Services.AddScoped<IEntityMapper<MODELS.Product, ENTITIES.Product>, ProductMapper>();
builder.Services.AddScoped<IEntityMapper<MODELS.ProductType, ENTITIES.ProductType>, ProductTypeMapper>();
builder.Services.AddScoped<IEntityMapper<MODELS.ProductVendor, ENTITIES.ProductVendor>, ProductVendorMapper>();
builder.Services.AddScoped<IEntityMapper<MODELS.Subscription, ENTITIES.Subscription>, SubscriptionMapper>();
builder.Services.AddScoped<IEntityMapper<MODELS.SubscriptionStripe, ENTITIES.SubscriptionStripe>, SubscriptionStripeMapper>();
builder.Services.AddScoped<IEntityMapper<MODELS.User, ENTITIES.User>, UserMapper>();
builder.Services.AddScoped<IEntityMapper<MODELS.UserAuthenticationApple, ENTITIES.UserAuthenticationApple>, UserAuthenticationAppleMapper>();
builder.Services.AddScoped<IEntityMapper<MODELS.UserAuthenticationGoogle, ENTITIES.UserAuthenticationGoogle>, UserAuthenticationGoogleMapper>();
builder.Services.AddScoped<IEntityMapper<MODELS.UserAuthenticationMicrosoft, ENTITIES.UserAuthenticationMicrosoft>, UserAuthenticationMicrosoftMapper>();
builder.Services.AddScoped<IEntityMapper<MODELS.UserRole, ENTITIES.UserRole>, UserRoleMapper>();
builder.Services.AddScoped<UserPreferencesJsonSerializer>();
builder.Services.AddScoped<AppleSignInHandler>();
builder.Services.AddScoped<EmailLookupService>();
builder.Services.AddScoped<GoogleSignInHandler>();
builder.Services.AddScoped<MicrosoftSignInHandler>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// --------------
// AUTHENTICATION
var authBuilder = builder.Services.AddAuthentication(options =>
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

    // GOOGLE SIGN-IN
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
        options.Events.OnRemoteFailure = context =>
        {
            context.Response.Redirect("/signin");
            context.HandleResponse();
            return Task.CompletedTask;
        };
    })

    // MICROSOFT SIGN-IN
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
        options.Events.OnRemoteFailure = context =>
        {
            context.Response.Redirect("/signin");
            context.HandleResponse();
            return Task.CompletedTask;
        };
    });

// APPLE SIGN-IN — only register when credentials are configured
var appleClientId = builder.Configuration["Authentication:Apple:ClientId"];
if (!string.IsNullOrEmpty(appleClientId))
{
    authBuilder.AddApple(options =>
    {
        options.ClientId = appleClientId;
        options.TeamId = builder.Configuration["Authentication:Apple:TeamId"]!;
        options.KeyId = builder.Configuration["Authentication:Apple:KeyId"]!;
        options.GenerateClientSecret = true;
        options.CallbackPath = "/signin-apple";
        options.PrivateKey = async (keyId, cancellationToken) =>
        {
            var keyContent = builder.Configuration["Authentication:Apple:PrivateKeyContent"];
            if (!string.IsNullOrEmpty(keyContent))
                return keyContent.AsMemory();

            var keyPath = builder.Configuration["Authentication:Apple:PrivateKeyPath"]!;
            return (await File.ReadAllTextAsync(keyPath, cancellationToken)).AsMemory();
        };
        options.Events.OnCreatingTicket = async context =>
        {
            var handler = context.HttpContext.RequestServices.GetRequiredService<APPLE_AUTH_HANDLER>();
            await handler.HandleTicketAsync(context);
        };
        options.Events.OnRemoteFailure = context =>
        {
            context.Response.Redirect("/signin");
            context.HandleResponse();
            return Task.CompletedTask;
        };
    });
}

builder.Services.AddScoped<APPLE_AUTH_HANDLER>();
builder.Services.AddScoped<GoogleAuthenticationHandler>();
builder.Services.AddScoped<MicrosoftAuthenticationHandler>();

// -------
// BILLING
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
builder.Services.AddScoped<StripeBillingService>();

// -----
// EMAIL
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.AddSingleton<IEmailService, AcsEmailService>();
builder.Services.AddExceptionHandler<ExceptionEmailHandler>();

// ----------------
// UNSUBSCRIBE TOKEN
builder.Services.Configure<UnsubscribeTokenSettings>(builder.Configuration.GetSection("UnsubscribeToken"));
builder.Services.AddScoped<UnsubscribeTokenService>(sp =>
{
    var settings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<UnsubscribeTokenSettings>>().Value;
    return new UnsubscribeTokenService(settings.SecretKey);
});

// -----
// ELMAH
builder.Services.AddElmah<SqlErrorLog>(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
    options.SqlServerDatabaseSchemaName = "components";
    options.OnPermissionCheck = context => context.User.Identity?.IsAuthenticated == true;
});

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

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