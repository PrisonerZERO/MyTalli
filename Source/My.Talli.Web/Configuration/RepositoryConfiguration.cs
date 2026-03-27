namespace My.Talli.Web.Configuration;

using Domain.Data.Interfaces;
using Web.Services.Identity;

/// <summary>Configuration</summary>
public static class RepositoryConfiguration
{
    #region <Methods>

    public static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<UserDisplayCache>();
    }

    #endregion
}
