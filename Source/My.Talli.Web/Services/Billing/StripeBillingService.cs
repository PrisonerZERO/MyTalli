namespace My.Talli.Web.Services.Billing;

using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

/// <summary>Service</summary>
public class StripeBillingService
{
    #region <Variables>

    private readonly ILogger<StripeBillingService> _logger;
    private readonly StripeSettings _settings;

    #endregion

    #region <Constructors>

    public StripeBillingService(ILogger<StripeBillingService> logger, IOptions<StripeSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
        StripeConfiguration.ApiKey = _settings.SecretKey;
    }

    #endregion

    #region <Properties>

    public string MonthlyPriceId => _settings.MonthlyPriceId;

    public string PublishableKey => _settings.PublishableKey;

    public string YearlyPriceId => _settings.YearlyPriceId;

    #endregion

    #region <Methods>

    public async Task<Session> CreateCheckoutSessionAsync(string userEmail, string priceId, string successUrl, string cancelUrl, long? userId = null)
    {
        var metadata = new Dictionary<string, string>();
        if (userId.HasValue)
            metadata["userId"] = userId.Value.ToString();

        var service = new SessionService();
        var options = ToOptions(metadata, userEmail, priceId, successUrl, cancelUrl, userId);
        var session = await service.CreateAsync(options);

        _logger.LogInformation("Created Stripe Checkout session {SessionId} for {Email}",session.Id, userEmail);

        return session;
    }

    public async Task<Stripe.BillingPortal.Session> CreatePortalSessionAsync(string stripeCustomerId, string returnUrl)
    {
        var options = new Stripe.BillingPortal.SessionCreateOptions { Customer = stripeCustomerId, ReturnUrl = returnUrl };
        var service = new Stripe.BillingPortal.SessionService();
        var session = await service.CreateAsync(options);

        _logger.LogInformation("Created Stripe Portal session for customer {CustomerId}", stripeCustomerId);

        return session;
    }

    public async Task SwitchPlanAsync(string stripeSubscriptionId, string newPriceId)
    {
        var subService = new SubscriptionService();
        var subscription = await subService.GetAsync(stripeSubscriptionId);

        if (subscription.Items?.Data is null || subscription.Items.Data.Count == 0)
            throw new InvalidOperationException("Subscription has no items to update.");

        var itemService = new SubscriptionItemService();
        var itemId = subscription.Items.Data[0].Id;        
        await itemService.UpdateAsync(itemId, new SubscriptionItemUpdateOptions { Price = newPriceId, ProrationBehavior = "create_prorations" });

        _logger.LogInformation("Switched subscription {SubscriptionId} to price {PriceId}", stripeSubscriptionId, newPriceId);
    }

    private SessionCreateOptions ToOptions(Dictionary<string, string> metadata, string userEmail, string priceId, string successUrl, string cancelUrl, long? userId = null)
    { 
        return new SessionCreateOptions
        {
            CancelUrl = cancelUrl,
            ClientReferenceId = userId?.ToString(),
            CustomerEmail = userEmail,
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Price = priceId,
                    Quantity = 1
                }
            ],
            Metadata = metadata,
            Mode = "subscription",
            SubscriptionData = new SessionSubscriptionDataOptions { Metadata = metadata },
            SuccessUrl = successUrl
        };
    }

    #endregion
}
