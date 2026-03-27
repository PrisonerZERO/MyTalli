namespace My.Talli.Web.Configuration;

using Web.Services.UI;

/// <summary>Configuration</summary>
public static class KnownIssueConfiguration
{
    #region <Methods>

    public static void AddKnownIssue(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KnownIssueSettings>(configuration.GetSection("KnownIssue"));
    }

    #endregion
}
