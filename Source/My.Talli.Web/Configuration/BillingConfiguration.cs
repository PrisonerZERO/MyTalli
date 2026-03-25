namespace My.Talli.Web.Configuration;

using Web.Commands.Endpoints;
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
        services.AddScoped<FindActiveSubscriptionWithStripeCommand>();
        services.AddScoped<StripeBillingService>();
        services.AddScoped<SubscriptionDeletedHandler>();
        services.AddScoped<SubscriptionUpdatedHandler>();
        services.AddScoped<UpdateLocalSubscriptionCommand>();

        Stripe.StripeConfiguration.ApiKey = stripeSection["SecretKey"];
    }

    #endregion
}
