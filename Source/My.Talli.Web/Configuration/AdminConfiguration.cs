namespace My.Talli.Web.Configuration;

using Web.Commands.Endpoints;
using Web.Commands.Notifications;

/// <summary>Configuration</summary>
public static class AdminConfiguration
{
    #region <Methods>

    public static void AddAdminServices(this IServiceCollection services)
    {
        services.AddScoped<GetAdminUserListCommand>();
        services.AddScoped<SendSubscriptionConfirmationEmailCommand>();
        services.AddScoped<SendWeeklySummaryEmailCommand>();
    }

    #endregion
}
