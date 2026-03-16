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
using My.Talli.Domain.Components.JsonSerializers;
using My.Talli.Domain.Components.Tokens;
using My.Talli.Domain.Data.EntityFramework;
using My.Talli.Domain.Data.EntityFramework.Repositories;
using My.Talli.Domain.Data.EntityFramework.Resolvers;
using My.Talli.Domain.Data.Interfaces;
using My.Talli.Domain.Handlers.Authentication;
using My.Talli.Domain.Mappers;
using My.Talli.Domain.Notifications.Emails;
using My.Talli.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using My.Talli.Web.Services.Email;
using My.Talli.Web.Services.Identity;
using My.Talli.Web.Services.Tokens;
using ElmahCore.Sql;
using ElmahCore.Mvc;

using APPLEAUTHHANDLER = My.Talli.Web.Services.Authentication.AppleAuthenticationHandler;
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
    });

// Apple Sign-In — only register when credentials are configured
var appleClientId = builder.Configuration["Authentication:Apple:ClientId"];
if (!string.IsNullOrEmpty(appleClientId))
{
    authBuilder.AddApple(options =>
    {
        // Configuration Uses: dotnet user-secrets
        options.ClientId = appleClientId;
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
            var handler = context.HttpContext.RequestServices.GetRequiredService<APPLEAUTHHANDLER>();
            await handler.HandleTicketAsync(context);
        };
    });
}

builder.Services.AddScoped<APPLEAUTHHANDLER>();
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

// -----------------
// EMAIL PREFERENCES
app.MapPost("/api/email/preferences", async (
    HttpContext context,
    ICurrentUserService currentUserService,
    UnsubscribeTokenService tokenService,
    RepositoryAdapterAsync<My.Talli.Domain.Models.User, My.Talli.Domain.Entities.User> userAdapter,
    UserPreferencesJsonSerializer preferencesSerializer) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var json = await reader.ReadToEndAsync();
    var request = System.Text.Json.JsonSerializer.Deserialize<EmailPreferencesRequest>(json);

    if (request is null || string.IsNullOrWhiteSpace(request.Token))
        return Results.BadRequest("Invalid request.");

    var userId = tokenService.ValidateToken(request.Token);
    if (userId is null)
        return Results.BadRequest("Invalid or expired token.");

    currentUserService.Set(userId.Value, "unsubscribe");

    var user = await userAdapter.GetByIdAsync(userId.Value);
    if (user is null)
        return Results.BadRequest("User not found.");

    var preferences = preferencesSerializer.Deserialize(user.UserPreferences);
    preferences.EmailPreferences.SubscriptionConfirmationEmail = request.SubscriptionConfirmationEmail;
    preferences.EmailPreferences.UnsubscribeAll = request.UnsubscribeAll;
    preferences.EmailPreferences.WeeklySummaryEmail = request.WeeklySummaryEmail;
    user.UserPreferences = preferencesSerializer.Serialize(preferences);

    await userAdapter.UpdateAsync(user);

    return Results.Ok();
}).DisableAntiforgery();

// ----------
// TEST EMAILS (Development only)
if (app.Environment.IsDevelopment())
{
    app.MapGet("/api/test/unsubscribe-token/{userId:long}", (long userId, UnsubscribeTokenService tokenService) =>
    {
        var token = tokenService.GenerateToken(userId);
        return Results.Text(token);
    });

    app.MapGet("/api/test/emails", async (IEmailService emailService, UnsubscribeTokenService tokenService) =>
    {
        var testRecipient = "hello@mytalli.com";
        var testToken = tokenService.GenerateToken(1);

        // 1. Waitlist Welcome Email
        var waitlistNotification = new WaitlistWelcomeEmailNotification();
        var waitlistEmail = waitlistNotification.Build(new EmailNotificationArgumentOf<WaitlistWelcomeEmailNotificationPayload>
        {
            Payload = new WaitlistWelcomeEmailNotificationPayload { FirstName = "Robert", UnsubscribeToken = testToken }
        });
        waitlistEmail.To = [testRecipient];
        await emailService.SendAsync(waitlistEmail);

        // 2. Welcome Email
        var welcomeNotification = new WelcomeEmailNotification();
        var welcomeEmail = welcomeNotification.Build(new EmailNotificationArgumentOf<WelcomeEmailNotificationPayload>
        {
            Payload = new WelcomeEmailNotificationPayload { FirstName = "Robert", UnsubscribeToken = testToken }
        });
        welcomeEmail.To = [testRecipient];
        await emailService.SendAsync(welcomeEmail);

        // 3. Subscription Confirmation Email
        var subNotification = new SubscriptionConfirmationEmailNotification();
        var subEmail = subNotification.Build(new EmailNotificationArgumentOf<SubscriptionConfirmationEmailNotificationPayload>
        {
            Payload = new SubscriptionConfirmationEmailNotificationPayload
            {
                Amount = "$12.00/mo",
                CardLastFour = "4242",
                FirstName = "Robert",
                Plan = "Pro",
                RenewalDate = "April 14, 2026",
                UnsubscribeToken = testToken
            }
        });
        subEmail.To = [testRecipient];
        await emailService.SendAsync(subEmail);

        // 4. Weekly Summary Email
        var summaryNotification = new WeeklySummaryEmailNotification();
        var summaryEmail = summaryNotification.Build(new EmailNotificationArgumentOf<WeeklySummaryEmailNotificationPayload>
        {
            Payload = new WeeklySummaryEmailNotificationPayload
            {
                FirstName = "Robert",
                UnsubscribeToken = testToken,
                GoalCurrent = "$2,847.00",
                GoalPercent = "57%",
                GoalRemaining = "$2,153.00",
                GoalTarget = "$5,000.00",
                TotalRevenue = "$1,247.50",
                TrendDirection = "\u25b2",
                TrendPercent = "12.4%",
                WeekRange = "Mar 7 \u2013 Mar 13, 2026",
                PlatformRows = @"
                    <tr>
                        <td style=""padding: 12px 0; border-bottom: 1px solid #f0edff;"">
                            <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
                                <tr>
                                    <td width=""4"" style=""background: #635bff; border-radius: 2px;""></td>
                                    <td style=""padding-left: 12px;"">
                                        <p style=""color: #1a1a2e; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 14px; font-weight: 600; margin: 0;"">Stripe</p>
                                        <p style=""color: #999; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 12px; margin: 2px 0 0;"">18 transactions</p>
                                    </td>
                                    <td align=""right"" valign=""top"">
                                        <p style=""color: #1a1a2e; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 14px; font-weight: 600; margin: 0;"">$842.00</p>
                                        <p style=""color: #27ae60; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 12px; margin: 2px 0 0;"">&#9650; 8.2%</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 12px 0; border-bottom: 1px solid #f0edff;"">
                            <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
                                <tr>
                                    <td width=""4"" style=""background: #f56400; border-radius: 2px;""></td>
                                    <td style=""padding-left: 12px;"">
                                        <p style=""color: #1a1a2e; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 14px; font-weight: 600; margin: 0;"">Etsy</p>
                                        <p style=""color: #999; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 12px; margin: 2px 0 0;"">7 transactions</p>
                                    </td>
                                    <td align=""right"" valign=""top"">
                                        <p style=""color: #1a1a2e; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 14px; font-weight: 600; margin: 0;"">$283.50</p>
                                        <p style=""color: #27ae60; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 12px; margin: 2px 0 0;"">&#9650; 22.1%</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 12px 0;"">
                            <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
                                <tr>
                                    <td width=""4"" style=""background: #ff90e8; border-radius: 2px;""></td>
                                    <td style=""padding-left: 12px;"">
                                        <p style=""color: #1a1a2e; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 14px; font-weight: 600; margin: 0;"">Gumroad</p>
                                        <p style=""color: #999; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 12px; margin: 2px 0 0;"">3 transactions</p>
                                    </td>
                                    <td align=""right"" valign=""top"">
                                        <p style=""color: #1a1a2e; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 14px; font-weight: 600; margin: 0;"">$122.00</p>
                                        <p style=""color: #e74c3c; font-family: 'DM Sans', Arial, Helvetica, sans-serif; font-size: 12px; margin: 2px 0 0;"">&#9660; 5.3%</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>"
            }
        });
        summaryEmail.To = [testRecipient];
        await emailService.SendAsync(summaryEmail);

        return Results.Text("4 test emails sent to hello@mytalli.com");
    });

    app.MapGet("/api/test/error", () =>
    {
        throw new InvalidOperationException("Test exception — verifying error email pipeline is working.");
    });
}

app.Run();

record EmailPreferencesRequest(
    string Token,
    bool SubscriptionConfirmationEmail,
    bool UnsubscribeAll,
    bool WeeklySummaryEmail);