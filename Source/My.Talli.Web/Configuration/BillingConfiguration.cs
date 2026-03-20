namespace My.Talli.Web.Configuration;

using Web.Services.Billing;

/// <summary>Configuration</summary>
public static class BillingConfiguration
{
    #region <Methods>

    public static void AddBillingServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<StripeSettings>(configuration.GetSection("Stripe"));
        services.AddScoped<StripeBillingService>();
    }

    #endregion
}
