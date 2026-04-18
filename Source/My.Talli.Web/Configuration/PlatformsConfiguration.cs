namespace My.Talli.Web.Configuration;

using Web.Services.Platforms;

/// <summary>Configuration</summary>
public static class PlatformsConfiguration
{
    #region <Methods>

    public static void AddPlatformServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EtsySettings>(configuration.GetSection("Etsy"));
        services.AddHttpClient<EtsyService>();
    }

    #endregion
}
