namespace My.Talli.Web.Endpoints;

using System.Security.Claims;
using Web.Services.Billing;

/// <summary>Endpoint</summary>
public static class BillingEndpoints
{
    #region <Methods>

    public static void MapBillingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/billing/create-checkout-session", async (HttpContext context, StripeBillingService billing) =>
        {
            var email = context.User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                return Results.Unauthorized();

            var plan = context.Request.Query["plan"].ToString();
            var priceId = plan == "yearly"
                ? billing.GetYearlyPriceId()
                : billing.GetMonthlyPriceId();

            var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
            var session = await billing.CreateCheckoutSessionAsync(
                email,
                priceId,
                successUrl: $"{baseUrl}/upgrade?status=success",
                cancelUrl: $"{baseUrl}/upgrade?status=cancelled");

            context.Response.Redirect(session.Url);
            return Results.Empty;
        }).RequireAuthorization();

        app.MapPost("/api/billing/create-portal-session", async (HttpContext context, StripeBillingService billing) =>
        {
            var email = context.User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                return Results.Unauthorized();

            // TODO: Look up Stripe customer ID from database using authenticated user
            // var stripeCustomerId = await userService.GetStripeCustomerIdAsync(userId);
            // var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
            // var session = await billing.CreatePortalSessionAsync(stripeCustomerId, $"{baseUrl}/subscription/cancel?status=kept");
            // context.Response.Redirect(session.Url);

            return Results.BadRequest("Portal not yet available — no database.");
        }).RequireAuthorization().DisableAntiforgery();

        app.MapPost("/api/billing/webhook", async (HttpContext context, IConfiguration config) =>
        {
            var webhookSecret = config["Stripe:WebhookSecret"];
            var json = await new StreamReader(context.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = Stripe.EventUtility.ConstructEvent(
                    json,
                    context.Request.Headers["Stripe-Signature"],
                    webhookSecret);

                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

                switch (stripeEvent.Type)
                {
                    case Stripe.EventTypes.CheckoutSessionCompleted:
                        logger.LogInformation("Checkout completed: {EventId}", stripeEvent.Id);
                        // TODO: Save Stripe customer ID to user record in database
                        // TODO: Set user's subscription tier to Pro
                        break;

                    case Stripe.EventTypes.CustomerSubscriptionUpdated:
                        logger.LogInformation("Subscription updated: {EventId}", stripeEvent.Id);
                        // TODO: Update user's subscription status in database
                        break;

                    case Stripe.EventTypes.CustomerSubscriptionDeleted:
                        logger.LogInformation("Subscription cancelled: {EventId}", stripeEvent.Id);
                        // TODO: Downgrade user to Free tier in database
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

    #endregion
}
