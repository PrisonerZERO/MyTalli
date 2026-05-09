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

    public static async Task PreWarmDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TalliDbContext>();

        try
        {
            _ = db.Model;
            await db.Database.CanConnectAsync();
        }
        catch (Exception ex)
        {
            app.Logger.LogWarning(ex, "DbContext pre-warm failed at startup; first request will pay the cold-start cost.");
        }
    }

    #endregion
}
