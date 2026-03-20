namespace My.Talli.Web.Configuration;

using Domain.Data.EntityFramework;
using Microsoft.EntityFrameworkCore;

/// <summary>Configuration</summary>
public static class DatabaseConfiguration
{
    #region <Methods>

    public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TalliDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
    }

    #endregion
}
