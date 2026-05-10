namespace My.Talli.Web.Commands.Notifications;

using Domain.Components.Tokens;
using Domain.Framework;
using Domain.Models;
using Domain.Notifications.Emails;
using Domain.Repositories;
using Microsoft.Extensions.Options;
using Web.Services.Billing;
using Web.Services.Email;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class SendSubscriptionConfirmationEmailCommand
{
    #region <Variables>

    private readonly RepositoryAdapterAsync<Billing, ENTITIES.Billing> _billingAdapter;
    private readonly RepositoryAdapterAsync<BillingStripe, ENTITIES.BillingStripe> _billingStripeAdapter;
    private readonly IEmailService _emailService;
    private readonly ILogger<SendSubscriptionConfirmationEmailCommand> _logger;
    private readonly RepositoryAdapterAsync<Product, ENTITIES.Product> _productAdapter;
    private readonly StripeSettings _stripeSettings;
    private readonly RepositoryAdapterAsync<Subscription, ENTITIES.Subscription> _subscriptionAdapter;
    private readonly RepositoryAdapterAsync<SubscriptionStripe, ENTITIES.SubscriptionStripe> _subscriptionStripeAdapter;
    private readonly UnsubscribeTokenService _tokenService;

    #endregion

    #region <Constructors>

    public SendSubscriptionConfirmationEmailCommand(
        RepositoryAdapterAsync<Billing, ENTITIES.Billing> billingAdapter,
        RepositoryAdapterAsync<BillingStripe, ENTITIES.BillingStripe> billingStripeAdapter,
        IEmailService emailService,
        ILogger<SendSubscriptionConfirmationEmailCommand> logger,
        RepositoryAdapterAsync<Product, ENTITIES.Product> productAdapter,
        IOptions<StripeSettings> stripeSettings,
        RepositoryAdapterAsync<Subscription, ENTITIES.Subscription> subscriptionAdapter,
        RepositoryAdapterAsync<SubscriptionStripe, ENTITIES.SubscriptionStripe> subscriptionStripeAdapter,
        UnsubscribeTokenService tokenService)
    {
        _billingAdapter = billingAdapter;
        _billingStripeAdapter = billingStripeAdapter;
        _emailService = emailService;
        _logger = logger;
        _productAdapter = productAdapter;
        _stripeSettings = stripeSettings.Value;
        _subscriptionAdapter = subscriptionAdapter;
        _subscriptionStripeAdapter = subscriptionStripeAdapter;
        _tokenService = tokenService;
    }

    #endregion

    #region <Methods>

    public async Task<bool> ExecuteAsync(string email, string firstName, long userId)
    {
        try
        {
            // Find active subscription
            var subscriptions = await _subscriptionAdapter.FindAsync(s =>
                s.UserId == userId && (s.Status == SubscriptionStatuses.Active || s.Status == SubscriptionStatuses.Cancelling));
            var subscription = subscriptions.FirstOrDefault();

            if (subscription is null)
            {
                _logger.LogWarning("No active subscription found for user {UserId} — skipping subscription confirmation email", userId);
                return false;
            }

            // Subscription Stripe data
            var subscriptionStripe = await _subscriptionStripeAdapter.GetByIdAsync(subscription.Id);

            // Product data
            var product = await _productAdapter.GetByIdAsync(subscription.ProductId);

            // Billing data (most recent for this user)
            var billings = await _billingAdapter.FindAsync(b => b.UserId == userId);
            var latestBilling = billings.OrderByDescending(b => b.Id).FirstOrDefault();

            // Card info
            var cardLastFour = "****";

            if (latestBilling is not null)
            {
                var billingStripe = await _billingStripeAdapter.GetByIdAsync(latestBilling.Id);

                if (billingStripe is not null)
                    cardLastFour = billingStripe.CardLastFour;
            }

            // Determine plan and amount
            var isYearly = subscriptionStripe is not null && subscriptionStripe.StripePriceId == _stripeSettings.YearlyPriceId;
            var amount = isYearly ? $"${product?.VendorPrice ?? 99m:F2}/yr" : $"${product?.VendorPrice ?? 12m:F2}/mo";
            var plan = "Pro";
            var renewalDate = subscription.RenewalDate.ToString("MMMM d, yyyy");

            // Build and send
            var notification = new SubscriptionConfirmationEmailNotification();
            var unsubscribeToken = _tokenService.GenerateToken(userId);
            var payload = new SubscriptionConfirmationEmailNotificationPayload
            {
                Amount = amount,
                CardLastFour = cardLastFour,
                FirstName = firstName,
                Plan = plan,
                RenewalDate = renewalDate,
                UnsubscribeToken = unsubscribeToken
            };
            var argument = new EmailNotificationArgumentOf<SubscriptionConfirmationEmailNotificationPayload> { Payload = payload };
            var smtp = notification.Build(argument);

            smtp.To = [email];
            await _emailService.SendAsync(smtp);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send subscription confirmation email for user {UserId}", userId);
            return false;
        }
    }

    #endregion
}
