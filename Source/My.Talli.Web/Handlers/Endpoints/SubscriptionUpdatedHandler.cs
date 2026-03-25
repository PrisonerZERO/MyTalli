namespace My.Talli.Web.Handlers.Endpoints;

using Domain.Framework;
using Domain.Handlers.Billing;
using Microsoft.Extensions.Options;
using Web.Services.Billing;

/// <summary>Handler</summary>
public class SubscriptionUpdatedHandler
{
    #region <Variables>

    private readonly ILogger<SubscriptionUpdatedHandler> _logger;
    private readonly StripeSettings _settings;
    private readonly StripeWebhookHandler _stripeWebhookHandler;

    #endregion

    #region <Constructors>

    public SubscriptionUpdatedHandler(ILogger<SubscriptionUpdatedHandler> logger, IOptions<StripeSettings> settings, StripeWebhookHandler stripeWebhookHandler)
    {
        _logger = logger;
        _settings = settings.Value;
        _stripeWebhookHandler = stripeWebhookHandler;
    }

    #endregion

    #region <Methods>

    public async Task HandleAsync(Stripe.Event stripeEvent)
    {
        try
        {
            var subscription = (Stripe.Subscription)stripeEvent.Data.Object;
            var payload = ToPayload(subscription);

            // TRANSACTION
            await EnforcedTransactionScope.ExecuteAsync(async () => await _stripeWebhookHandler.HandleSubscriptionUpdatedAsync(payload));

            _logger.LogInformation("Subscription updated: {SubscriptionId}, status: {Status}", subscription.Id, subscription.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing customer.subscription.updated event {EventId}", stripeEvent.Id);
        }
    }

    private SubscriptionUpdatedPayload ToPayload(Stripe.Subscription subscription)
    {
        var stripePriceId = string.Empty;
        var currentPeriodEnd = DateTime.UtcNow.AddMonths(1);

        if (subscription.Items?.Data?.Count > 0)
        {
            stripePriceId = subscription.Items.Data[0].Price?.Id ?? string.Empty;
            currentPeriodEnd = subscription.Items.Data[0].CurrentPeriodEnd;
        }

        string? productName = null;
        if (!string.IsNullOrEmpty(stripePriceId))
            productName = stripePriceId == _settings.YearlyPriceId ? "Pro Yearly" : "Pro Monthly";

        return new SubscriptionUpdatedPayload
        {
            CancelAtPeriodEnd = subscription.CancelAtPeriodEnd,
            CurrentPeriodEnd = currentPeriodEnd,
            ProductName = productName,
            Status = subscription.Status,
            StripePriceId = stripePriceId,
            StripeSubscriptionId = subscription.Id
        };
    }

    #endregion
}
