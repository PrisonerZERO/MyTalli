namespace My.Talli.Web.Handlers.Endpoints;

using Domain.Framework;
using Domain.Handlers.Billing;

/// <summary>Handler</summary>
public class SubscriptionDeletedHandler
{
    #region <Variables>

    private readonly ILogger<SubscriptionDeletedHandler> _logger;
    private readonly StripeWebhookHandler _stripeWebhookHandler;

    #endregion

    #region <Constructors>

    public SubscriptionDeletedHandler(ILogger<SubscriptionDeletedHandler> logger, StripeWebhookHandler stripeWebhookHandler)
    {
        _logger = logger;
        _stripeWebhookHandler = stripeWebhookHandler;
    }

    #endregion

    #region <Methods>

    public async Task HandleAsync(Stripe.Event stripeEvent)
    {
        try
        {
            var subscription = (Stripe.Subscription)stripeEvent.Data.Object;
            var payload = new SubscriptionDeletedPayload { StripeSubscriptionId = subscription.Id };

            // TRANSACTION
            await EnforcedTransactionScope.ExecuteAsync(async () => await _stripeWebhookHandler.HandleSubscriptionDeletedAsync(payload));

            _logger.LogInformation("Subscription deleted: {SubscriptionId}", subscription.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing customer.subscription.deleted event {EventId}", stripeEvent.Id);
        }
    }

    #endregion
}
