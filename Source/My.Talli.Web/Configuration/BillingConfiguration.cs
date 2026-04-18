namespace My.Talli.Web.Configuration;

using Web.Handlers.Endpoints;
using Web.Services.Billing;

/// <summary>Configuration</summary>
public static class BillingConfiguration
{
    #region <Methods>

    public static void AddBillingServices(this IServiceCollection services, IConfiguration configuration)
    {
        var stripeSection = configuration.GetSection("Stripe");

        services.Configure<StripeSettings>(stripeSection);
        services.AddScoped<CheckoutCompletedHandler>();
        services.AddScoped<StripeBillingService>();
        services.AddScoped<SubscriptionDeletedHandler>();
        services.AddScoped<SubscriptionUpdatedHandler>();

        Stripe.StripeConfiguration.ApiKey = stripeSection["SecretKey"];
    }

    #endregion
}
