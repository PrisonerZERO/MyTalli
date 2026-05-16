namespace My.Talli.Web.Configuration;

using Domain.Data.EntityFramework;
using Microsoft.AspNetCore.DataProtection;
using Web.Services.Platforms;
using Web.Workers;

/// <summary>Configuration</summary>
public static class PlatformsConfiguration
{
    #region <Methods>

    public static void AddPlatformServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EtsySettings>(configuration.GetSection("Etsy"));
        services.AddHttpClient<EtsyService>();
        services.AddScoped<IEtsyApiClient>(sp => sp.GetRequiredService<EtsyService>());

        services.Configure<GumroadSettings>(configuration.GetSection("Gumroad"));
        services.AddHttpClient<GumroadService>();
        services.AddScoped<IGumroadApiClient>(sp => sp.GetRequiredService<GumroadService>());

        services.Configure<StripeConnectSettings>(configuration.GetSection("StripeConnect"));
        services.AddScoped<StripeConnectService>();
        services.AddScoped<IStripeConnectApiClient>(sp => sp.GetRequiredService<StripeConnectService>());

        // Persist DataProtection keys (used for auth cookies + antiforgery + Stripe Connect onboarding cookie)
        // to SQL via TalliDbContext (components.DataProtectionKey table). Survives container restarts,
        // slot swaps, and scale events; no MI / blob / sidecar dependency.
        services.AddDataProtection().PersistKeysToDbContext<TalliDbContext>();

        // Per-platform token refreshers (rotate refresh tokens before expiry)
        services.AddScoped<EtsyTokenRefresher>();
        services.AddScoped<IPlatformTokenRefresher, EtsyTokenRefresher>(sp => sp.GetRequiredService<EtsyTokenRefresher>());

        // Per-platform sync services (pull revenue data)
        services.AddScoped<IPlatformSyncService, EtsySyncService>();
        services.AddScoped<IPlatformSyncService, GumroadSyncService>();
        services.AddScoped<IPlatformSyncService, StripeConnectSyncService>();

        // Background workers
        services.AddHostedService<TokenRefreshWorker>();
        services.AddHostedService<ShopSyncWorker>();
    }

    #endregion
}
