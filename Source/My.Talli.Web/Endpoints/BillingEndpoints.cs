namespace My.Talli.Web.Endpoints;

using Domain.Components.Tokens;
using Domain.Framework;
using Domain.Handlers.Billing;
using Domain.Models;
using Domain.Notifications.Emails;
using Domain.Repositories;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Web.Services.Billing;
using Web.Services.Email;

using ENTITIES = Domain.Entities;

/// <summary>Endpoint</summary>
public static class BillingEndpoints
{
    #region <Methods>

    public static void MapBillingEndpoints(this IEndpointRouteBuilder app)
    {
        // ENDPOINT - Create Checkout Session
        app.MapGet("/api/billing/create-checkout-session", async (HttpContext context, StripeBillingService billing) =>
        {
            var email = context.User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                return Results.Unauthorized();

            long? userId = null;
            var userIdClaim = context.User.FindFirst("UserId")?.Value;
            if (long.TryParse(userIdClaim, out var parsed))
                userId = parsed;

            var plan = context.Request.Query["plan"].ToString();
            var priceId = plan == "yearly" ? billing.YearlyPriceId : billing.MonthlyPriceId;

            var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
            var session = await billing.CreateCheckoutSessionAsync(email, priceId, successUrl: $"{baseUrl}/upgrade?status=success", cancelUrl: $"{baseUrl}/upgrade?status=cancelled", userId: userId);

            context.Response.Redirect(session.Url);
            return Results.Empty;
        }).RequireAuthorization();

        // ENDPOINT - Create Portal Session
        app.MapGet("/api/billing/create-portal-session", async (HttpContext context, StripeBillingService billing) =>
        {
            var userIdClaim = context.User.FindFirst("UserId")?.Value;
            if (!long.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            var (subscription, stripeRecord) = await FindActiveSubscriptionWithStripeAsync(context, userId);
            if (subscription is null || stripeRecord is null)
            {
                context.Response.Redirect("/upgrade");
                return Results.Empty;
            }

            var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
            var session = await billing.CreatePortalSessionAsync(stripeRecord.StripeCustomerId, $"{baseUrl}/subscription");

            context.Response.Redirect(session.Url);
            return Results.Empty;
        }).RequireAuthorization();

        // ENDPOINT - Switch Plan
        app.MapGet("/api/billing/switch-plan", async (HttpContext context, StripeBillingService billing) =>
        {
            var userIdClaim = context.User.FindFirst("UserId")?.Value;
            if (!long.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            var plan = context.Request.Query["plan"].ToString();
            if (plan != "monthly" && plan != "yearly")
            {
                context.Response.Redirect("/upgrade");
                return Results.Empty;
            }

            var (subscription, stripeRecord) = await FindActiveSubscriptionWithStripeAsync(context, userId);
            if (subscription is null || stripeRecord is null)
            {
                context.Response.Redirect("/upgrade");
                return Results.Empty;
            }

            var newPriceId = plan == "yearly" ? billing.YearlyPriceId : billing.MonthlyPriceId;
            await billing.SwitchPlanAsync(stripeRecord.StripeSubscriptionId, newPriceId);

            // UPDATE - Don't wait for webhook
            await UpdateLocalSubscriptionAsync(context, subscription, stripeRecord, plan, newPriceId);

            context.Response.Redirect("/upgrade?status=switched");
            return Results.Empty;
        }).RequireAuthorization();

        // ENDPOINT - Billing Webhook
        app.MapPost("/api/billing/webhook", async (HttpContext context) =>
        {
            var config = context.RequestServices.GetRequiredService<IConfiguration>();
            var webhookSecret = config["Stripe:WebhookSecret"];
            var json = await new StreamReader(context.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = Stripe.EventUtility.ConstructEvent(json, context.Request.Headers["Stripe-Signature"], webhookSecret);
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

                switch (stripeEvent.Type)
                {
                    case Stripe.EventTypes.CheckoutSessionCompleted:
                        await HandleCheckoutCompletedAsync(stripeEvent, context, logger);
                        break;

                    case Stripe.EventTypes.CustomerSubscriptionUpdated:
                        await HandleSubscriptionUpdatedAsync(stripeEvent, context, logger);
                        break;

                    case Stripe.EventTypes.CustomerSubscriptionDeleted:
                        await HandleSubscriptionDeletedAsync(stripeEvent, context, logger);
                        break;

                    default:
                        logger.LogDebug("Unhandled Stripe event type: {Type}", stripeEvent.Type);
                        break;
                }

                return Results.Ok();
            }
            catch (Stripe.StripeException ex)
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning(ex, "Stripe webhook signature verification failed");
                return Results.BadRequest();
            }
        }).DisableAntiforgery();
    }

    private static async Task HandleCheckoutCompletedAsync(Stripe.Event stripeEvent, HttpContext context, ILogger logger)
    {
        try
        {
            var session = (Stripe.Checkout.Session)stripeEvent.Data.Object;
            var settings = context.RequestServices.GetRequiredService<IOptions<StripeSettings>>().Value;
            var payload = await ToCheckoutCompletedPayloadAsync(session, settings);
            var handler = context.RequestServices.GetRequiredService<StripeWebhookHandler>();

            // TRANSACTION
            var result = await EnforcedTransactionScope.ExecuteAsync(async () => await handler.HandleCheckoutCompletedAsync(payload));

            // Log & Email
            logger.LogInformation("Checkout completed for user {UserId}, plan {Plan}", result.UserId, result.Plan);
            await SendConfirmationEmailAsync(context, result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing checkout.session.completed event {EventId}", stripeEvent.Id);
        }
    }

    private static async Task HandleSubscriptionDeletedAsync(Stripe.Event stripeEvent, HttpContext context, ILogger logger)
    {
        try
        {
            var subscription = (Stripe.Subscription)stripeEvent.Data.Object;
            var handler = context.RequestServices.GetRequiredService<StripeWebhookHandler>();
            var payload = new SubscriptionDeletedPayload { StripeSubscriptionId = subscription.Id };

            // TRANSACTION
            await EnforcedTransactionScope.ExecuteAsync(async () => await handler.HandleSubscriptionDeletedAsync(payload));

            logger.LogInformation("Subscription deleted: {SubscriptionId}", subscription.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing customer.subscription.deleted event {EventId}", stripeEvent.Id);
        }
    }

    private static async Task HandleSubscriptionUpdatedAsync(Stripe.Event stripeEvent, HttpContext context, ILogger logger)
    {
        try
        {
            var subscription = (Stripe.Subscription)stripeEvent.Data.Object;
            var settings = context.RequestServices.GetRequiredService<IOptions<StripeSettings>>().Value;
            var payload = ToSubscriptionUpdatedPayload(subscription, settings);
            var handler = context.RequestServices.GetRequiredService<StripeWebhookHandler>();

            // TRANSACTION
            await EnforcedTransactionScope.ExecuteAsync(async () => await handler.HandleSubscriptionUpdatedAsync(payload));

            logger.LogInformation("Subscription updated: {SubscriptionId}, status: {Status}", subscription.Id, subscription.Status);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing customer.subscription.updated event {EventId}", stripeEvent.Id);
        }
    }

    private static async Task<(Subscription?, SubscriptionStripe?)> FindActiveSubscriptionWithStripeAsync(HttpContext context, long userId)
    {
        var subscriptionAdapter = context.RequestServices.GetRequiredService<RepositoryAdapterAsync<Subscription, ENTITIES.Subscription>>();
        var subscriptionStripeAdapter = context.RequestServices.GetRequiredService<RepositoryAdapterAsync<SubscriptionStripe, ENTITIES.SubscriptionStripe>>();

        var subscription = (await subscriptionAdapter.FindAsync(x => x.UserId == userId && (x.Status == "Active" || x.Status == "Cancelling"))).FirstOrDefault();
        if (subscription is null)
            return (null, null);

        var stripeRecord = await subscriptionStripeAdapter.GetByIdAsync(subscription.Id);
        return (subscription, stripeRecord);
    }

    private static async Task SendConfirmationEmailAsync(HttpContext context, CheckoutCompletedResult result)
    {
        try
        {
            if (string.IsNullOrEmpty(result.UserEmail))
                return;

            var emailService = context.RequestServices.GetRequiredService<IEmailService>();
            var tokenService = context.RequestServices.GetRequiredService<UnsubscribeTokenService>();
            var smtp = ToConfirmationSmtp(result, tokenService);

            smtp.To = [result.UserEmail];
            await emailService.SendAsync(smtp);
        }
        catch (Exception ex)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Failed to send subscription confirmation email for user {UserId}", result.UserId);
        }
    }

    private static async Task<CheckoutCompletedPayload> ToCheckoutCompletedPayloadAsync(Stripe.Checkout.Session session, StripeSettings settings)
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
            ProductName = stripePriceId == settings.YearlyPriceId ? "Pro Yearly" : "Pro Monthly",
            StripeCustomerId = stripeCustomerId,
            StripePaymentIntentId = paymentIntentId,
            StripePriceId = stripePriceId,
            StripeSubscriptionId = stripeSubscriptionId,
            UserId = userId
        };
    }

    private static SmtpNotification ToConfirmationSmtp(CheckoutCompletedResult result, UnsubscribeTokenService tokenService)
    {
        return new SubscriptionConfirmationEmailNotification().Build(new EmailNotificationArgumentOf<SubscriptionConfirmationEmailNotificationPayload>
        {
            Payload = new SubscriptionConfirmationEmailNotificationPayload
            {
                Amount = result.Amount,
                CardLastFour = result.CardLastFour,
                FirstName = result.UserFirstName,
                Plan = result.Plan,
                RenewalDate = result.RenewalDate,
                UnsubscribeToken = tokenService.GenerateToken(result.UserId)
            }
        });
    }

    private static SubscriptionUpdatedPayload ToSubscriptionUpdatedPayload(Stripe.Subscription subscription, StripeSettings settings)
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
            productName = stripePriceId == settings.YearlyPriceId ? "Pro Yearly" : "Pro Monthly";

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

    private static async Task UpdateLocalSubscriptionAsync(HttpContext context, Subscription subscription, SubscriptionStripe stripeRecord, string plan, string newPriceId)
    {
        var subscriptionAdapter = context.RequestServices.GetRequiredService<RepositoryAdapterAsync<Subscription, ENTITIES.Subscription>>();
        var subscriptionStripeAdapter = context.RequestServices.GetRequiredService<RepositoryAdapterAsync<SubscriptionStripe, ENTITIES.SubscriptionStripe>>();
        var productAdapter = context.RequestServices.GetRequiredService<RepositoryAdapterAsync<Product, ENTITIES.Product>>();

        var newProductName = plan == "yearly" ? "Pro Yearly" : "Pro Monthly";
        var newProduct = (await productAdapter.FindAsync(x => x.ProductName == newProductName)).FirstOrDefault();

        // TRANSACTION
        await EnforcedTransactionScope.ExecuteAsync(async () =>
        {
            if (newProduct is not null)
            {
                subscription.ProductId = newProduct.Id;
                await subscriptionAdapter.UpdateAsync(subscription);
            }

            stripeRecord.StripePriceId = newPriceId;
            await subscriptionStripeAdapter.UpdateAsync(stripeRecord);
        });
    }

    #endregion
}
