namespace My.Talli.Web.Configuration;

using Domain.Data.EntityFramework;
using Microsoft.EntityFrameworkCore;

/// <summary>Configuration</summary>
public static class DatabaseConfiguration
{
    #region <Methods>

    public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<TalliDbContext>(options => options.UseSqlServer(connectionString));

        // IDbContextFactory<TalliDbContext> registers separately and produces brand-new DbContext instances
        // on each CreateDbContextAsync() call. Use this when you need to run multiple reads in parallel via
        // Task.WhenAll — each task gets its own context so they don't contend on the scoped TalliDbContext's
        // ConcurrencyLock. See GetExportPreviewCommand for the reference usage.
        services.AddDbContextFactory<TalliDbContext>(options => options.UseSqlServer(connectionString));
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
