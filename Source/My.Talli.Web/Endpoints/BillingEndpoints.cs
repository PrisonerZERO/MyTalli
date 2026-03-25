namespace My.Talli.Web.Endpoints;

using Domain.Framework;
using Domain.Models;
using Domain.Repositories;
using System.Security.Claims;
using Web.Handlers.Endpoints;
using Web.Services.Billing;

using ENTITIES = Domain.Entities;

/// <summary>Endpoint</summary>
public static class BillingEndpoints
{
    #region <Endpoints>

    public static void MapBillingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/billing/create-checkout-session", CreateCheckoutSession).RequireAuthorization();
        app.MapGet("/api/billing/create-portal-session", CreatePortalSession).RequireAuthorization();
        app.MapGet("/api/billing/switch-plan", SwitchPlan).RequireAuthorization();
        app.MapPost("/api/billing/webhook", (Delegate)Webhook).DisableAntiforgery();
    }

    #endregion

    #region <Methods>

    private static async Task<IResult> CreateCheckoutSession(HttpContext context, StripeBillingService billing)
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
    }

    private static async Task<IResult> CreatePortalSession(HttpContext context, StripeBillingService billing)
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
    }

    private static async Task<IResult> SwitchPlan(HttpContext context, StripeBillingService billing)
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
    }

    private static async Task<IResult> Webhook(HttpContext context)
    {
        var config = context.RequestServices.GetRequiredService<IConfiguration>();
        var webhookSecret = config["Stripe:WebhookSecret"];
        var json = await new StreamReader(context.Request.Body).ReadToEndAsync();

        try
        {
            var stripeEvent = Stripe.EventUtility.ConstructEvent(json, context.Request.Headers["Stripe-Signature"], webhookSecret);
            var handler = context.RequestServices.GetRequiredService<BillingWebhookHandler>();

            switch (stripeEvent.Type)
            {
                case Stripe.EventTypes.CheckoutSessionCompleted:
                    await handler.HandleCheckoutCompletedAsync(stripeEvent);
                    break;

                case Stripe.EventTypes.CustomerSubscriptionUpdated:
                    await handler.HandleSubscriptionUpdatedAsync(stripeEvent);
                    break;

                case Stripe.EventTypes.CustomerSubscriptionDeleted:
                    await handler.HandleSubscriptionDeletedAsync(stripeEvent);
                    break;

                default:
                    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
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
