using System.Security.Claims;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using AspNet.Security.OAuth.Apple;
using My.Talli.Web.Components;
using My.Talli.Web.Services.Authentication;
using My.Talli.Web.Services.Billing;
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

// -------
// BILLING
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
builder.Services.AddScoped<StripeBillingService>();

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// Build
var app = builder.Build();

// ---------------------
// HTTP REQUEST PIPELINE
app.UseExceptionHandler("/Error", createScopeForErrors: true);
app.UseStatusCodePagesWithReExecute("/Error/{0}");

if (!app.Environment.IsDevelopment())
{
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
        RedirectUri = "/dashboard"
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
    var name = context.User.Identity?.Name;
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    var redirect = string.IsNullOrEmpty(name)
        ? "/?signed-out"
        : $"/?signed-out&name={Uri.EscapeDataString(name)}";

    context.Response.Redirect(redirect);
});

// -------
// BILLING
app.MapGet("/api/billing/create-checkout-session", async (HttpContext context, StripeBillingService billing) =>
{
    var email = context.User.FindFirstValue(ClaimTypes.Email);
    if (string.IsNullOrEmpty(email))
        return Results.Unauthorized();

    var plan = context.Request.Query["plan"].ToString();
    var priceId = plan == "yearly"
        ? billing.GetYearlyPriceId()
        : billing.GetMonthlyPriceId();

    var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
    var session = await billing.CreateCheckoutSessionAsync(
        email,
        priceId,
        successUrl: $"{baseUrl}/upgrade?status=success",
        cancelUrl: $"{baseUrl}/upgrade?status=cancelled");

    context.Response.Redirect(session.Url);
    return Results.Empty;
}).RequireAuthorization();

app.MapPost("/api/billing/create-portal-session", async (HttpContext context, StripeBillingService billing) =>
{
    var email = context.User.FindFirstValue(ClaimTypes.Email);
    if (string.IsNullOrEmpty(email))
        return Results.Unauthorized();

    // TODO: Look up Stripe customer ID from database using authenticated user
    // var stripeCustomerId = await userService.GetStripeCustomerIdAsync(userId);
    // var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
    // var session = await billing.CreatePortalSessionAsync(stripeCustomerId, $"{baseUrl}/subscription/cancel?status=kept");
    // context.Response.Redirect(session.Url);

    return Results.BadRequest("Portal not yet available — no database.");
}).RequireAuthorization().DisableAntiforgery();

app.MapPost("/api/billing/webhook", async (HttpContext context, IConfiguration config) =>
{
    var webhookSecret = config["Stripe:WebhookSecret"];
    var json = await new StreamReader(context.Request.Body).ReadToEndAsync();

    try
    {
        var stripeEvent = Stripe.EventUtility.ConstructEvent(
            json,
            context.Request.Headers["Stripe-Signature"],
            webhookSecret);

        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

        switch (stripeEvent.Type)
        {
            case Stripe.EventTypes.CheckoutSessionCompleted:
                logger.LogInformation("Checkout completed: {EventId}", stripeEvent.Id);
                // TODO: Save Stripe customer ID to user record in database
                // TODO: Set user's subscription tier to Pro
                break;

            case Stripe.EventTypes.CustomerSubscriptionUpdated:
                logger.LogInformation("Subscription updated: {EventId}", stripeEvent.Id);
                // TODO: Update user's subscription status in database
                break;

            case Stripe.EventTypes.CustomerSubscriptionDeleted:
                logger.LogInformation("Subscription cancelled: {EventId}", stripeEvent.Id);
                // TODO: Downgrade user to Free tier in database
                break;

            default:
                logger.LogDebug("Unhandled Stripe event type: {Type}", stripeEvent.Type);
                break;
        }

        return Results.Ok();
    }
    catch (Stripe.StripeException ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Stripe webhook signature verification failed");
        return Results.BadRequest();
    }
}).DisableAntiforgery();

app.Run();