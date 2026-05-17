namespace My.Talli.Web.Configuration;

using Web.Commands.Billing;
using Web.Handlers.Endpoints;
using Web.Services.Billing;
using Web.Workers;

/// <summary>Configuration</summary>
public static class BillingConfiguration
{
    #region <Methods>

    public static void AddBillingServices(this IServiceCollection services, IConfiguration configuration)
    {
        var stripeSection = configuration.GetSection("Stripe");

        services.Configure<StripeSettings>(stripeSection);
        services.AddScoped<CheckoutCompletedHandler>();
        services.AddScoped<ReconcileBillingHealthCommand>();
        services.AddScoped<StripeBillingService>();
        services.AddScoped<IStripeBillingApiClient>(sp => sp.GetRequiredService<StripeBillingService>());
        services.AddScoped<SubscriptionDeletedHandler>();
        services.AddScoped<SubscriptionUpdatedHandler>();

        // BillingHealthWorker — reconciles local Subscriptions vs Stripe once per day, logs drift to Elmah
        services.AddHostedService<BillingHealthWorker>();

        Stripe.StripeConfiguration.ApiKey = stripeSection["SecretKey"];
    }

    #endregion
}
