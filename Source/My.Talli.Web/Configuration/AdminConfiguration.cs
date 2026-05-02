namespace My.Talli.Web.Configuration;

using Web.Commands.Endpoints;
using Web.Commands.Notifications;
using Web.Services.Admin;
using Web.Workers;

/// <summary>Configuration</summary>
public static class AdminConfiguration
{
    #region <Methods>

    public static void AddAdminServices(this IServiceCollection services)
    {
        services.AddScoped<GetAdminUserListCommand>();
        services.AddScoped<SendSubscriptionConfirmationEmailCommand>();
        services.AddScoped<SendWeeklySummaryEmailCommand>();

        // Maintenance Mode — singleton holds the cached flag, initializer primes it from DB at boot
        services.AddSingleton<IMaintenanceModeService, MaintenanceModeService>();
        services.AddHostedService<MaintenanceModeStartupInitializer>();

        // Heartbeat — AdminHealthWorker ticks every minute, refreshes MM cache + writes own liveness row
        services.AddHostedService<AdminHealthWorker>();
    }

    #endregion
}
