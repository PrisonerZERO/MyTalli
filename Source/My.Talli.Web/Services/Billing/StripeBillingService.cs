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

    public StripeBillingService(
        ILogger<StripeBillingService> logger,
        IOptions<StripeSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
        StripeConfiguration.ApiKey = _settings.SecretKey;
    }


    #endregion

    #region <Methods>

    public async Task<Session> CreateCheckoutSessionAsync(
        string userEmail,
        string priceId,
        string successUrl,
        string cancelUrl,
        long? userId = null)
    {
        var metadata = new Dictionary<string, string>();
        if (userId.HasValue)
            metadata["userId"] = userId.Value.ToString();

        var options = new SessionCreateOptions
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

        var service = new SessionService();
        var session = await service.CreateAsync(options);

        _logger.LogInformation(
            "Created Stripe Checkout session {SessionId} for {Email}",
            session.Id, userEmail);

        return session;
    }

    public async Task<Stripe.BillingPortal.Session> CreatePortalSessionAsync(
        string stripeCustomerId,
        string returnUrl)
    {
        var options = new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = stripeCustomerId,
            ReturnUrl = returnUrl
        };

        var service = new Stripe.BillingPortal.SessionService();
        var session = await service.CreateAsync(options);

        _logger.LogInformation(
            "Created Stripe Portal session for customer {CustomerId}",
            stripeCustomerId);

        return session;
    }

    public string GetMonthlyPriceId() => _settings.MonthlyPriceId;

    public string GetPublishableKey() => _settings.PublishableKey;

    public string GetYearlyPriceId() => _settings.YearlyPriceId;


    #endregion
}
