namespace My.Talli.Web.Configuration;

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

        // Per-platform token refreshers (rotate refresh tokens before expiry)
        services.AddScoped<EtsyTokenRefresher>();
        services.AddScoped<IPlatformTokenRefresher, EtsyTokenRefresher>(sp => sp.GetRequiredService<EtsyTokenRefresher>());

        // Per-platform sync services (pull revenue data)
        services.AddScoped<IPlatformSyncService, EtsySyncService>();

        // Background workers
        services.AddHostedService<TokenRefreshWorker>();
        services.AddHostedService<ShopSyncWorker>();
    }

    #endregion
}
