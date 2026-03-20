namespace My.Talli.Web.Configuration;

using ElmahCore.Mvc;
using ElmahCore.Sql;

/// <summary>Configuration</summary>
public static class ElmahConfiguration
{
    #region <Methods>

    public static void AddElmahLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddElmah<SqlErrorLog>(options =>
        {
            options.ConnectionString = configuration.GetConnectionString("DefaultConnection")!;
            options.SqlServerDatabaseSchemaName = "components";
            options.OnPermissionCheck = context => context.User.Identity?.IsAuthenticated == true;
        });
    }

    #endregion
}
