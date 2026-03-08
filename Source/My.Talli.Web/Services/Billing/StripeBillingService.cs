using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace My.Talli.Web.Services.Billing;

public class StripeBillingService
{
    #region <Variables>

    private readonly ILogger<StripeBillingService> _logger;
    private readonly StripeSettings _settings;

    #endregion

    #region <Constructors>

    public StripeBillingService(
        IOptions<StripeSettings> settings,
        ILogger<StripeBillingService> logger)
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
        string cancelUrl)
    {
        var options = new SessionCreateOptions
        {
            CustomerEmail = userEmail,
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Price = priceId,
                    Quantity = 1
                }
            ],
            Mode = "subscription",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl
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
