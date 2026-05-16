namespace My.Talli.UnitTesting.Handlers.Billing;

using Domain.Framework.Exceptions;

using FRAMEWORK = Domain.Framework;
using Domain.Handlers.Authentication;
using Domain.Handlers.Billing;
using Domain.Models;
using My.Talli.UnitTesting.Infrastructure.Builders;

/// <summary>Tests</summary>
public class StripeWebhookHandlerTests
{
	#region <Methods>

	// ── HandleCheckoutCompletedAsync ──

	[Fact]
	public async Task CheckoutWithUserId_SkipsEmailLookup()
	{
		var builder = new BillingHandlerBuilder();
		var user = await SeedUserAsync(builder);
		var product = await SeedProductAsync(builder, "Pro Monthly Subscription", 12m);

		var payload = CreateCheckoutPayload(product.Id, "unknown@nowhere.com");
		payload.UserId = user.Id;

		var result = await builder.Handler.HandleCheckoutCompletedAsync(payload);

		Assert.Equal(user.Id, result.UserId);
	}

	[Fact]
	public async Task DuplicateCheckout_DoesNotCreateNewRecords()
	{
		var builder = new BillingHandlerBuilder();
		var user = await SeedUserAsync(builder);
		var product = await SeedProductAsync(builder, "Pro Monthly Subscription", 12m);

		var payload = CreateCheckoutPayload(product.Id, "test@gmail.com");
		payload.UserId = user.Id;

		await builder.Handler.HandleCheckoutCompletedAsync(payload);
		await builder.Handler.HandleCheckoutCompletedAsync(payload);

		var orders = await builder.OrderAdapter.GetAllAsync();
		Assert.Single(orders);
	}

	[Fact]
	public async Task DuplicateCheckout_ReturnsExistingSubscription()
	{
		var builder = new BillingHandlerBuilder();
		var user = await SeedUserAsync(builder);
		var product = await SeedProductAsync(builder, "Pro Monthly Subscription", 12m);

		var payload = CreateCheckoutPayload(product.Id, "test@gmail.com");
		payload.UserId = user.Id;

		var first = await builder.Handler.HandleCheckoutCompletedAsync(payload);
		var second = await builder.Handler.HandleCheckoutCompletedAsync(payload);

		Assert.Equal(first.Plan, second.Plan);
		Assert.Equal(first.UserId, second.UserId);
	}

	[Fact]
	public async Task NewCheckout_CreatesBilling()
	{
		var builder = new BillingHandlerBuilder();
		var user = await SeedUserAsync(builder);
		var product = await SeedProductAsync(builder, "Pro Monthly Subscription", 12m);

		var payload = CreateCheckoutPayload(product.Id, "test@gmail.com");
		payload.UserId = user.Id;

		await builder.Handler.HandleCheckoutCompletedAsync(payload);

		var billing = Assert.Single(await builder.BillingAdapter.GetAllAsync());
		Assert.Equal(12m, billing.Amount);
		Assert.Equal("usd", billing.Currency);
		Assert.Equal("Paid", billing.Status);
		Assert.Equal(user.Id, billing.UserId);
	}

	[Fact]
	public async Task NewCheckout_CreatesBillingStripe()
	{
		var builder = new BillingHandlerBuilder();
		var user = await SeedUserAsync(builder);
		var product = await SeedProductAsync(builder, "Pro Monthly Subscription", 12m);

		var payload = CreateCheckoutPayload(product.Id, "test@gmail.com");
		payload.UserId = user.Id;

		await builder.Handler.HandleCheckoutCompletedAsync(payload);

		var billingStripe = Assert.Single(await builder.BillingStripeAdapter.GetAllAsync());
		Assert.Equal("visa", billingStripe.CardBrand);
		Assert.Equal("4242", billingStripe.CardLastFour);
		Assert.Equal("pi_test_123", billingStripe.StripePaymentIntentId);
		Assert.Equal("pm_card_visa", billingStripe.PaymentMethod);
	}

	[Fact]
	public async Task NewCheckout_CreatesOrder()
	{
		var builder = new BillingHandlerBuilder();
		var user = await SeedUserAsync(builder);
		var product = await SeedProductAsync(builder, "Pro Monthly Subscription", 12m);

		var payload = CreateCheckoutPayload(product.Id, "test@gmail.com");
		payload.UserId = user.Id;

		await builder.Handler.HandleCheckoutCompletedAsync(payload);

		var order = Assert.Single(await builder.OrderAdapter.GetAllAsync());
		Assert.Equal(user.Id, order.UserId);
		Assert.Equal(0m, order.TaxCharged);
	}

	[Fact]
	public async Task NewCheckout_CreatesOrderItem()
	{
		var builder = new BillingHandlerBuilder();
		var user = await SeedUserAsync(builder);
		var product = await SeedProductAsync(builder, "Pro Monthly Subscription", 12m);

		var payload = CreateCheckoutPayload(product.Id, "test@gmail.com");
		payload.UserId = user.Id;

		await builder.Handler.HandleCheckoutCompletedAsync(payload);

		var orderItem = Assert.Single(await builder.OrderItemAdapter.GetAllAsync());
		Assert.Equal(product.Id, orderItem.ProductId);
		Assert.Equal(12m, orderItem.ProductPriceCharged);
		Assert.Equal(1, orderItem.ProductQuantity);
	}

	[Fact]
	public async Task NewCheckout_CreatesSubscription()
	{
		var builder = new BillingHandlerBuilder();
		var user = await SeedUserAsync(builder);
		var product = await SeedProductAsync(builder, "Pro Monthly Subscription", 12m);

		var payload = CreateCheckoutPayload(product.Id, "test@gmail.com");
		payload.UserId = user.Id;

		await builder.Handler.HandleCheckoutCompletedAsync(payload);

		var subscription = Assert.Single(await builder.SubscriptionAdapter.GetAllAsync());
		Assert.Equal(FRAMEWORK.SubscriptionStatuses.Active, subscription.Status);
		Assert.Equal(user.Id, subscription.UserId);
		Assert.Equal(product.Id, subscription.ProductId);
		Assert.Equal(payload.CurrentPeriodEnd, subscription.EndDate);
		Assert.Equal(payload.CurrentPeriodEnd, subscription.RenewalDate);
	}

	[Fact]
	public async Task NewCheckout_CreatesSubscriptionStripe()
	{
		var builder = new BillingHandlerBuilder();
		var user = await SeedUserAsync(builder);
		var product = await SeedProductAsync(builder, "Pro Monthly Subscription", 12m);

		var payload = CreateCheckoutPayload(product.Id, "test@gmail.com");
		payload.UserId = user.Id;

		await builder.Handler.HandleCheckoutCompletedAsync(payload);

		var subscriptionStripe = Assert.Single(await builder.SubscriptionStripeAdapter.GetAllAsync());
		Assert.Equal("sub_test_123", subscriptionStripe.StripeSubscriptionId);
		Assert.Equal("cus_test_123", subscriptionStripe.StripeCustomerId);
		Assert.Equal("price_monthly", subscriptionStripe.StripePriceId);
	}

	[Fact]
	public async Task NewCheckout_ReturnsResult()
	{
		var builder = new BillingHandlerBuilder();
		var user = await SeedUserAsync(builder);
		var product = await SeedProductAsync(builder, "Pro Monthly Subscription", 12m);

		var payload = CreateCheckoutPayload(product.Id, "test@gmail.com");
		payload.UserId = user.Id;

		var result = await builder.Handler.HandleCheckoutCompletedAsync(payload);

		Assert.Equal("Pro", result.Plan);
		Assert.Equal("$12.00/mo", result.Amount);
		Assert.Equal("4242", result.CardLastFour);
		Assert.Equal(user.Id, result.UserId);
		Assert.Equal("test@gmail.com", result.UserEmail);
	}

	[Fact]
	public async Task NewCheckout_SetsCurrentUser()
	{
		var builder = new BillingHandlerBuilder();
		var user = await SeedUserAsync(builder);
		var product = await SeedProductAsync(builder, "Pro Monthly Subscription", 12m);

		var payload = CreateCheckoutPayload(product.Id, "test@gmail.com");
		payload.UserId = user.Id;

		await builder.Handler.HandleCheckoutCompletedAsync(payload);

		Assert.Equal(user.Id, builder.CurrentUserService.UserId);
	}

	[Fact]
	public async Task UnknownEmail_ThrowsNotFoundException()
	{
		var builder = new BillingHandlerBuilder();
		var product = await SeedProductAsync(builder, "Pro Monthly Subscription", 12m);

		var payload = CreateCheckoutPayload(product.Id, "nobody@nowhere.com");

		await Assert.ThrowsAsync<NotFoundException>(() => builder.Handler.HandleCheckoutCompletedAsync(payload));
	}

	[Fact]
	public async Task UnknownProduct_ThrowsNotFoundException()
	{
		var builder = new BillingHandlerBuilder();
		var user = await SeedUserAsync(builder);

		var payload = CreateCheckoutPayload(999, "test@gmail.com");
		payload.UserId = user.Id;

		await Assert.ThrowsAsync<NotFoundException>(() => builder.Handler.HandleCheckoutCompletedAsync(payload));
	}

	// ── Module checkout (ProductId >= 3) ──

	[Fact]
	public async Task ModuleCheckout_CreatesSubscriptionWithModuleProductId()
	{
		var builder = new BillingHandlerBuilder();
		var user = await SeedUserAsync(builder);
		var module = await SeedProductAsync(builder, "Manual Entry Module", 3m, productTypeId: 2);

		var payload = CreateCheckoutPayload(module.Id, "test@gmail.com");
		payload.UserId = user.Id;
		payload.StripeSubscriptionId = "sub_module_123";
		payload.StripePriceId = "price_module_3";

		await builder.Handler.HandleCheckoutCompletedAsync(payload);

		var subscription = Assert.Single(await builder.SubscriptionAdapter.GetAllAsync());
		Assert.Equal(module.Id, subscription.ProductId);
		Assert.Equal(FRAMEWORK.SubscriptionStatuses.Active, subscription.Status);
		Assert.Equal(user.Id, subscription.UserId);
	}

	[Fact]
	public async Task ModuleCheckout_CreatesBillingAtModulePrice()
	{
		var builder = new BillingHandlerBuilder();
		var user = await SeedUserAsync(builder);
		var module = await SeedProductAsync(builder, "Manual Entry Module", 3m, productTypeId: 2);

		var payload = CreateCheckoutPayload(module.Id, "test@gmail.com");
		payload.UserId = user.Id;
		payload.StripeSubscriptionId = "sub_module_123";

		await builder.Handler.HandleCheckoutCompletedAsync(payload);

		var billing = Assert.Single(await builder.BillingAdapter.GetAllAsync());
		Assert.Equal(3m, billing.Amount);
	}

	[Fact]
	public async Task ProUserAddsModule_CreatesSecondSubscriptionRowNotReplacingPro()
	{
		// User has an active Pro subscription, then buys a module. Module checkout creates a NEW
		// (Subscription + SubscriptionStripe) row alongside Pro — separate Stripe subscription id, separate
		// row. The existing-stripe early-return path in HandleCheckoutCompletedAsync only fires on duplicate
		// of the SAME Stripe subscription id, so a different sub_xxx must produce a new row.
		var builder = new BillingHandlerBuilder();
		var user = await SeedUserAsync(builder);
		var pro = await SeedProductAsync(builder, "Pro Monthly Subscription", 12m);
		var module = await SeedProductAsync(builder, "Manual Entry Module", 3m, productTypeId: 2);

		// Pro checkout
		var proPayload = CreateCheckoutPayload(pro.Id, "test@gmail.com");
		proPayload.UserId = user.Id;
		proPayload.StripeSubscriptionId = "sub_pro_123";
		await builder.Handler.HandleCheckoutCompletedAsync(proPayload);

		// Module checkout (separate Stripe subscription)
		var modulePayload = CreateCheckoutPayload(module.Id, "test@gmail.com");
		modulePayload.UserId = user.Id;
		modulePayload.StripeSubscriptionId = "sub_module_456";
		modulePayload.StripePriceId = "price_module_3";
		await builder.Handler.HandleCheckoutCompletedAsync(modulePayload);

		var subscriptions = (await builder.SubscriptionAdapter.GetAllAsync()).ToList();
		Assert.Equal(2, subscriptions.Count);
		Assert.Contains(subscriptions, s => s.ProductId == pro.Id);
		Assert.Contains(subscriptions, s => s.ProductId == module.Id);
	}

	// ── HandleSubscriptionDeletedAsync ──

	[Fact]
	public async Task DeletedExistingSubscription_SetsCancelled()
	{
		var builder = new BillingHandlerBuilder();
		await SeedCheckoutAsync(builder);

		await builder.Handler.HandleSubscriptionDeletedAsync(new SubscriptionDeletedPayload
		{
			StripeSubscriptionId = "sub_test_123"
		});

		var subscription = Assert.Single(await builder.SubscriptionAdapter.GetAllAsync());
		Assert.Equal(FRAMEWORK.SubscriptionStatuses.Cancelled, subscription.Status);
		Assert.NotNull(subscription.CancelledDate);
	}

	[Fact]
	public async Task DeletedUnknownSubscription_DoesNothing()
	{
		var builder = new BillingHandlerBuilder();

		await builder.Handler.HandleSubscriptionDeletedAsync(new SubscriptionDeletedPayload
		{
			StripeSubscriptionId = "sub_unknown"
		});

		var subscriptions = await builder.SubscriptionAdapter.GetAllAsync();
		Assert.Empty(subscriptions);
	}

	// ── HandleSubscriptionUpdatedAsync ──

	[Fact]
	public async Task UpdateCancelAtPeriodEnd_SetsCancelling()
	{
		var builder = new BillingHandlerBuilder();
		await SeedCheckoutAsync(builder);

		await builder.Handler.HandleSubscriptionUpdatedAsync(new SubscriptionUpdatedPayload
		{
			CancelAtPeriodEnd = true,
			CurrentPeriodEnd = DateTime.UtcNow.AddDays(30),
			Status = "active",
			StripeSubscriptionId = "sub_test_123"
		});

		var subscription = Assert.Single(await builder.SubscriptionAdapter.GetAllAsync());
		Assert.Equal(FRAMEWORK.SubscriptionStatuses.Cancelling, subscription.Status);
		Assert.NotNull(subscription.CancelledDate);
	}

	[Fact]
	public async Task UpdatePriceChange_UpdatesStripeRecord()
	{
		var builder = new BillingHandlerBuilder();
		await SeedCheckoutAsync(builder);

		await builder.Handler.HandleSubscriptionUpdatedAsync(new SubscriptionUpdatedPayload
		{
			CancelAtPeriodEnd = false,
			CurrentPeriodEnd = DateTime.UtcNow.AddDays(365),
			Status = "active",
			StripePriceId = "price_yearly",
			StripeSubscriptionId = "sub_test_123"
		});

		var stripeRecord = Assert.Single(await builder.SubscriptionStripeAdapter.GetAllAsync());
		Assert.Equal("price_yearly", stripeRecord.StripePriceId);
	}

	[Fact]
	public async Task UpdatePriceChangeWithProduct_UpdatesProductId()
	{
		var builder = new BillingHandlerBuilder();
		await SeedCheckoutAsync(builder);
		var yearlyProduct = await SeedProductAsync(builder, "Pro Yearly Subscription", 99m);

		await builder.Handler.HandleSubscriptionUpdatedAsync(new SubscriptionUpdatedPayload
		{
			CancelAtPeriodEnd = false,
			CurrentPeriodEnd = DateTime.UtcNow.AddDays(365),
			ProductId = yearlyProduct.Id,
			Status = "active",
			StripePriceId = "price_yearly",
			StripeSubscriptionId = "sub_test_123"
		});

		var subscription = Assert.Single(await builder.SubscriptionAdapter.GetAllAsync());
		Assert.Equal(yearlyProduct.Id, subscription.ProductId);
	}

	[Fact]
	public async Task UpdateReactivation_ClearsCancelledDate()
	{
		var builder = new BillingHandlerBuilder();
		await SeedCheckoutAsync(builder);

		// First cancel
		await builder.Handler.HandleSubscriptionUpdatedAsync(new SubscriptionUpdatedPayload
		{
			CancelAtPeriodEnd = true,
			CurrentPeriodEnd = DateTime.UtcNow.AddDays(30),
			Status = "active",
			StripeSubscriptionId = "sub_test_123"
		});

		// Then reactivate
		await builder.Handler.HandleSubscriptionUpdatedAsync(new SubscriptionUpdatedPayload
		{
			CancelAtPeriodEnd = false,
			CurrentPeriodEnd = DateTime.UtcNow.AddDays(30),
			Status = "active",
			StripeSubscriptionId = "sub_test_123"
		});

		var subscription = Assert.Single(await builder.SubscriptionAdapter.GetAllAsync());
		Assert.Equal(FRAMEWORK.SubscriptionStatuses.Active, subscription.Status);
		Assert.Null(subscription.CancelledDate);
	}

	[Fact]
	public async Task UpdateStatusPastDue_MapsToPastDue()
	{
		var builder = new BillingHandlerBuilder();
		await SeedCheckoutAsync(builder);

		await builder.Handler.HandleSubscriptionUpdatedAsync(new SubscriptionUpdatedPayload
		{
			CancelAtPeriodEnd = false,
			CurrentPeriodEnd = DateTime.UtcNow.AddDays(30),
			Status = "past_due",
			StripeSubscriptionId = "sub_test_123"
		});

		var subscription = Assert.Single(await builder.SubscriptionAdapter.GetAllAsync());
		Assert.Equal(FRAMEWORK.SubscriptionStatuses.PastDue, subscription.Status);
	}

	[Fact]
	public async Task UpdateStatusUnpaid_MapsToUnpaid()
	{
		var builder = new BillingHandlerBuilder();
		await SeedCheckoutAsync(builder);

		await builder.Handler.HandleSubscriptionUpdatedAsync(new SubscriptionUpdatedPayload
		{
			CancelAtPeriodEnd = false,
			CurrentPeriodEnd = DateTime.UtcNow.AddDays(30),
			Status = "unpaid",
			StripeSubscriptionId = "sub_test_123"
		});

		var subscription = Assert.Single(await builder.SubscriptionAdapter.GetAllAsync());
		Assert.Equal(FRAMEWORK.SubscriptionStatuses.Unpaid, subscription.Status);
	}

	[Fact]
	public async Task UpdateUnknownSubscription_DoesNothing()
	{
		var builder = new BillingHandlerBuilder();

		await builder.Handler.HandleSubscriptionUpdatedAsync(new SubscriptionUpdatedPayload
		{
			CancelAtPeriodEnd = false,
			CurrentPeriodEnd = DateTime.UtcNow.AddDays(30),
			Status = "active",
			StripeSubscriptionId = "sub_unknown"
		});

		var subscriptions = await builder.SubscriptionAdapter.GetAllAsync();
		Assert.Empty(subscriptions);
	}

	// ── Helpers ──

	private static CheckoutCompletedPayload CreateCheckoutPayload(long productId, string email) => new()
	{
		CardBrand = "visa",
		CardLastFour = "4242",
		CurrentPeriodEnd = DateTime.UtcNow.AddDays(30),
		CustomerEmail = email,
		PaymentMethod = "pm_card_visa",
		ProductId = productId,
		StripeCustomerId = "cus_test_123",
		StripePaymentIntentId = "pi_test_123",
		StripePriceId = "price_monthly",
		StripeSubscriptionId = "sub_test_123"
	};

	private static async Task<Product> SeedProductAsync(BillingHandlerBuilder builder, string name, decimal price, long productTypeId = 1)
	{
		var vendor = await builder.ProductAdapter.InsertAsync(new Product
		{
			ProductName = name,
			ProductTypeId = productTypeId,
			VendorId = 1,
			VendorPrice = price
		});
		return vendor;
	}

	private static async Task SeedCheckoutAsync(BillingHandlerBuilder builder)
	{
		var user = await SeedUserAsync(builder);
		var product = await SeedProductAsync(builder, "Pro Monthly Subscription", 12m);

		var payload = CreateCheckoutPayload(product.Id, "test@gmail.com");
		payload.UserId = user.Id;

		await builder.Handler.HandleCheckoutCompletedAsync(payload);
	}

	private static async Task<User> SeedUserAsync(BillingHandlerBuilder builder)
	{
		var argument = new SignInArgumentOf<GoogleSignInPayload>
		{
			DisplayName = "Test User", Email = "test@gmail.com", FirstName = "Test", LastName = "User",
			Payload = new GoogleSignInPayload { AvatarUrl = "https://example.com/avatar.jpg", EmailVerified = true, GoogleId = "google-123", Locale = "en" }
		};
		return await builder.GoogleHandler.HandleAsync(argument);
	}

	#endregion
}
