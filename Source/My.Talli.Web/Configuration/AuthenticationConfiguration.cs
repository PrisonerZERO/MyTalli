namespace My.Talli.Web.Configuration;

using AspNet.Security.OAuth.Apple;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Web.Commands.Notifications;
using Web.Handlers.Authentication;

using APPLE_AUTH_HANDLER = Web.Handlers.Authentication.AppleAuthenticationHandler;

/// <summary>Configuration</summary>
public static class AuthenticationConfiguration
{
    #region <Methods>

    public static void AddAuthenticationProviders(this IServiceCollection services, IConfiguration configuration)
    {
        var authBuilder = services.AddAuthentication(options =>
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
                options.ClientId = configuration["Authentication:Google:ClientId"]!;
                options.ClientSecret = configuration["Authentication:Google:ClientSecret"]!;
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
                options.ClientId = configuration["Authentication:Microsoft:ClientId"]!;
                options.ClientSecret = configuration["Authentication:Microsoft:ClientSecret"]!;
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
        var appleClientId = configuration["Authentication:Apple:ClientId"];
        if (!string.IsNullOrEmpty(appleClientId))
        {
            authBuilder.AddApple(options =>
            {
                options.ClientId = appleClientId;
                options.TeamId = configuration["Authentication:Apple:TeamId"]!;
                options.KeyId = configuration["Authentication:Apple:KeyId"]!;
                options.GenerateClientSecret = true;
                options.CallbackPath = "/signin-apple";
                options.PrivateKey = async (keyId, cancellationToken) =>
                {
                    var keyContent = configuration["Authentication:Apple:PrivateKeyContent"];
                    if (!string.IsNullOrEmpty(keyContent))
                        return keyContent.AsMemory();

                    var keyPath = configuration["Authentication:Apple:PrivateKeyPath"]!;
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

        services.AddScoped<APPLE_AUTH_HANDLER>();
        services.AddScoped<GoogleAuthenticationHandler>();
        services.AddScoped<MicrosoftAuthenticationHandler>();
        services.AddScoped<SendWelcomeEmailCommand>();

        services.AddAuthorization();
        services.AddCascadingAuthenticationState();
    }

    #endregion
}
