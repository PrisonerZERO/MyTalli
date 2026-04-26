namespace My.Talli.Web.Configuration;

using Azure.Identity;
using Domain.Components.Tokens;
using Microsoft.AspNetCore.DataProtection;
using Web.Services.Platforms;
using Web.Services.Tokens;
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

        // At-rest encryption for OAuth tokens stored on ShopConnection.
        // In Azure, persist the master key to Blob Storage so it survives App Service VM moves and slot swaps.
        // Locally (no AccountName configured), fall back to the default file-system store.
        var dataProtection = services.AddDataProtection();

        var blobAccountName = configuration["DataProtection:BlobStorage:AccountName"];
        var blobContainerName = configuration["DataProtection:BlobStorage:ContainerName"];

        if (!string.IsNullOrWhiteSpace(blobAccountName) && !string.IsNullOrWhiteSpace(blobContainerName))
        {
            var blobUri = new Uri($"https://{blobAccountName}.blob.core.windows.net/{blobContainerName}/keys.xml");
            dataProtection.PersistKeysToAzureBlobStorage(blobUri, new DefaultAzureCredential());
        }

        services.AddSingleton<IShopTokenProtector, DataProtectionShopTokenProtector>();

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
