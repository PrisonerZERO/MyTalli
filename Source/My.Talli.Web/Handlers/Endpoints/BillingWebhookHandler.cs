namespace My.Talli.Web.Handlers.Endpoints;

using Domain.Components.Tokens;
using Domain.Framework;
using Domain.Handlers.Billing;
using Domain.Notifications.Emails;
using Microsoft.Extensions.Options;
using Web.Services.Billing;
using Web.Services.Email;

/// <summary>Handler</summary>
public class BillingWebhookHandler
{
    #region <Variables>

    private readonly IEmailService _emailService;
    private readonly ILogger<BillingWebhookHandler> _logger;
    private readonly StripeSettings _settings;
    private readonly StripeWebhookHandler _stripeWebhookHandler;
    private readonly UnsubscribeTokenService _tokenService;

    #endregion

    #region <Constructors>

    public BillingWebhookHandler(IEmailService emailService, ILogger<BillingWebhookHandler> logger, IOptions<StripeSettings> settings, StripeWebhookHandler stripeWebhookHandler, UnsubscribeTokenService tokenService)
    {
        _emailService = emailService;
        _logger = logger;
        _settings = settings.Value;
        _stripeWebhookHandler = stripeWebhookHandler;
        _tokenService = tokenService;
    }

    #endregion

    #region <Methods>

    public async Task HandleCheckoutCompletedAsync(Stripe.Event stripeEvent)
    {
        try
        {
            var session = (Stripe.Checkout.Session)stripeEvent.Data.Object;
            var payload = await ToCheckoutCompletedPayloadAsync(session);

            // TRANSACTION
            var result = await EnforcedTransactionScope.ExecuteAsync(async () => await _stripeWebhookHandler.HandleCheckoutCompletedAsync(payload));

            // Log & Email
            _logger.LogInformation("Checkout completed for user {UserId}, plan {Plan}", result.UserId, result.Plan);
            await SendConfirmationEmailAsync(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing checkout.session.completed event {EventId}", stripeEvent.Id);
        }
    }

    public async Task HandleSubscriptionDeletedAsync(Stripe.Event stripeEvent)
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

    public async Task HandleSubscriptionUpdatedAsync(Stripe.Event stripeEvent)
    {
        try
        {
            var subscription = (Stripe.Subscription)stripeEvent.Data.Object;
            var payload = ToSubscriptionUpdatedPayload(subscription);

            // TRANSACTION
            await EnforcedTransactionScope.ExecuteAsync(async () => await _stripeWebhookHandler.HandleSubscriptionUpdatedAsync(payload));

            _logger.LogInformation("Subscription updated: {SubscriptionId}, status: {Status}", subscription.Id, subscription.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing customer.subscription.updated event {EventId}", stripeEvent.Id);
        }
    }

    private async Task SendConfirmationEmailAsync(CheckoutCompletedResult result)
    {
        try
        {
            if (string.IsNullOrEmpty(result.UserEmail))
                return;

            var smtp = ToConfirmationSmtp(result);
            smtp.To = [result.UserEmail];
            await _emailService.SendAsync(smtp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send subscription confirmation email for user {UserId}", result.UserId);
        }
    }

    private async Task<CheckoutCompletedPayload> ToCheckoutCompletedPayloadAsync(Stripe.Checkout.Session session)
    {
        var stripeSubscriptionId = session.SubscriptionId ?? string.Empty;
        var stripeCustomerId = session.CustomerId ?? string.Empty;
        var customerEmail = session.CustomerEmail ?? session.CustomerDetails?.Email ?? string.Empty;

        long? userId = null;
        if (!string.IsNullOrEmpty(session.ClientReferenceId) && long.TryParse(session.ClientReferenceId, out var parsed))
            userId = parsed;

        var cardBrand = string.Empty;
        var cardLastFour = string.Empty;
        var paymentMethod = string.Empty;
        var paymentIntentId = string.Empty;
        var currentPeriodEnd = DateTime.UtcNow.AddMonths(1);
        var stripePriceId = string.Empty;

        if (!string.IsNullOrEmpty(stripeSubscriptionId))
        {
            var subService = new Stripe.SubscriptionService();
            var stripeSub = await subService.GetAsync(stripeSubscriptionId, new Stripe.SubscriptionGetOptions { Expand = ["default_payment_method"] });

            if (stripeSub.Items?.Data?.Count > 0)
            {
                var firstItem = stripeSub.Items.Data[0];
                stripePriceId = firstItem.Price?.Id ?? string.Empty;
                currentPeriodEnd = firstItem.CurrentPeriodEnd;
            }

            if (stripeSub.DefaultPaymentMethod is Stripe.PaymentMethod pm)
            {
                paymentMethod = pm.Type ?? string.Empty;
                cardBrand = pm.Card?.Brand ?? string.Empty;
                cardLastFour = pm.Card?.Last4 ?? string.Empty;
            }

            paymentIntentId = stripeSub.LatestInvoiceId ?? string.Empty;
        }

        return new CheckoutCompletedPayload
        {
            CardBrand = cardBrand,
            CardLastFour = cardLastFour,
            CurrentPeriodEnd = currentPeriodEnd,
            CustomerEmail = customerEmail,
            PaymentMethod = paymentMethod,
            ProductName = stripePriceId == _settings.YearlyPriceId ? "Pro Yearly" : "Pro Monthly",
            StripeCustomerId = stripeCustomerId,
            StripePaymentIntentId = paymentIntentId,
            StripePriceId = stripePriceId,
            StripeSubscriptionId = stripeSubscriptionId,
            UserId = userId
        };
    }

    private SmtpNotification ToConfirmationSmtp(CheckoutCompletedResult result)
    {
        var notification = new SubscriptionConfirmationEmailNotification();
        var unsubscribeToken = _tokenService.GenerateToken(result.UserId);
        var notificationPayload = new SubscriptionConfirmationEmailNotificationPayload
        {
            Amount = result.Amount,
            CardLastFour = result.CardLastFour,
            FirstName = result.UserFirstName,
            Plan = result.Plan,
            RenewalDate = result.RenewalDate,
            UnsubscribeToken = unsubscribeToken
        };
        var notificationArgument = new EmailNotificationArgumentOf<SubscriptionConfirmationEmailNotificationPayload> { Payload = notificationPayload };

        return notification.Build(notificationArgument);
    }

    private SubscriptionUpdatedPayload ToSubscriptionUpdatedPayload(Stripe.Subscription subscription)
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
