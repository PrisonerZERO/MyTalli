namespace My.Talli.Web.Configuration;

using ElmahCore;
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
            options.Filters.Add(new ElmahStatusCodeFilter());
        });
    }

    #endregion
}

/// <summary>Filter</summary>
public class ElmahStatusCodeFilter : IErrorFilter
{
    #region <Methods>

    public void OnErrorModuleFiltering(object sender, ExceptionFilterEventArgs args)
    {
        if (args.Context is HttpContext httpContext && httpContext.Response.StatusCode == 404)
            args.Dismiss();
    }

    #endregion
}
