namespace My.Talli.Domain.Handlers.Billing;

using Domain.Data.Interfaces;
using Domain.Framework;
using Domain.Framework.Exceptions;
using Domain.Handlers.Authentication;
using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Handler</summary>
public class StripeWebhookHandler
{
	#region <Variables>

	private readonly EmailLookupService _emailLookupService;
	private readonly ICurrentUserService _currentUserService;
	private readonly RepositoryAdapterAsync<Billing, ENTITIES.Billing> _billingAdapter;
	private readonly RepositoryAdapterAsync<BillingStripe, ENTITIES.BillingStripe> _billingStripeAdapter;
	private readonly RepositoryAdapterAsync<Order, ENTITIES.Order> _orderAdapter;
	private readonly RepositoryAdapterAsync<OrderItem, ENTITIES.OrderItem> _orderItemAdapter;
	private readonly RepositoryAdapterAsync<Product, ENTITIES.Product> _productAdapter;
	private readonly RepositoryAdapterAsync<Subscription, ENTITIES.Subscription> _subscriptionAdapter;
	private readonly RepositoryAdapterAsync<SubscriptionStripe, ENTITIES.SubscriptionStripe> _subscriptionStripeAdapter;
	private readonly RepositoryAdapterAsync<User, ENTITIES.User> _userAdapter;


	#endregion

	#region <Constructors>

	public StripeWebhookHandler(
		EmailLookupService emailLookupService,
		ICurrentUserService currentUserService,
		RepositoryAdapterAsync<Billing, ENTITIES.Billing> billingAdapter,
		RepositoryAdapterAsync<BillingStripe, ENTITIES.BillingStripe> billingStripeAdapter,
		RepositoryAdapterAsync<Order, ENTITIES.Order> orderAdapter,
		RepositoryAdapterAsync<OrderItem, ENTITIES.OrderItem> orderItemAdapter,
		RepositoryAdapterAsync<Product, ENTITIES.Product> productAdapter,
		RepositoryAdapterAsync<Subscription, ENTITIES.Subscription> subscriptionAdapter,
		RepositoryAdapterAsync<SubscriptionStripe, ENTITIES.SubscriptionStripe> subscriptionStripeAdapter,
		RepositoryAdapterAsync<User, ENTITIES.User> userAdapter)
	{
		_billingAdapter = billingAdapter;
		_billingStripeAdapter = billingStripeAdapter;
		_currentUserService = currentUserService;
		_emailLookupService = emailLookupService;
		_orderAdapter = orderAdapter;
		_orderItemAdapter = orderItemAdapter;
		_productAdapter = productAdapter;
		_subscriptionAdapter = subscriptionAdapter;
		_subscriptionStripeAdapter = subscriptionStripeAdapter;
		_userAdapter = userAdapter;
	}


	#endregion

	#region <Methods>

	public async Task<CheckoutCompletedResult> HandleCheckoutCompletedAsync(CheckoutCompletedPayload payload)
	{
		var userId = payload.UserId
			?? await _emailLookupService.FindUserIdByEmailAsync(payload.CustomerEmail);

		if (!userId.HasValue)
			throw new NotFoundException($"No user found for email {payload.CustomerEmail}");

		var existingStripe = (await _subscriptionStripeAdapter.FindAsync(
			x => x.StripeSubscriptionId == payload.StripeSubscriptionId)).FirstOrDefault();

		if (existingStripe is not null)
		{
			var existingSub = (await _subscriptionAdapter.GetByIdAsync(existingStripe.Id))!;
			var existingProduct = (await _productAdapter.GetByIdAsync(existingSub.ProductId))!;
			return BuildResult(userId.Value, string.Empty, payload.CustomerEmail, existingProduct, existingSub, payload.CardLastFour);
		}

		var user = (await _userAdapter.GetByIdAsync(userId.Value))!;
		_currentUserService.Set(user.Id, user.DisplayName);

		var product = (await _productAdapter.GetByIdAsync(payload.ProductId))
			?? throw new NotFoundException($"No product found for ID {payload.ProductId}");

		var order = await _orderAdapter.InsertAsync(new Order
		{
			OrderDateTime = DateTime.UtcNow,
			TaxCharged = 0,
			UserId = user.Id
		});

		var orderItem = await _orderItemAdapter.InsertAsync(new OrderItem
		{
			OrderId = order.Id,
			ProductId = product.Id,
			ProductPriceCharged = product.VendorPrice,
			ProductQuantity = 1
		});

		var subscription = await _subscriptionAdapter.InsertAsync(new Subscription
		{
			EndDate = payload.CurrentPeriodEnd,
			OrderItemId = orderItem.Id,
			ProductId = product.Id,
			RenewalDate = payload.CurrentPeriodEnd,
			StartDate = DateTime.UtcNow,
			Status = SubscriptionStatuses.Active,
			UserId = user.Id
		});

		await _subscriptionStripeAdapter.InsertAsync(new SubscriptionStripe
		{
			Id = subscription.Id,
			StripeCustomerId = payload.StripeCustomerId,
			StripePriceId = payload.StripePriceId,
			StripeSubscriptionId = payload.StripeSubscriptionId
		});

		var billing = await _billingAdapter.InsertAsync(new Billing
		{
			Amount = product.VendorPrice,
			Currency = "usd",
			OrderId = order.Id,
			Status = "Paid",
			UserId = user.Id
		});

		await _billingStripeAdapter.InsertAsync(new BillingStripe
		{
			CardBrand = payload.CardBrand,
			CardLastFour = payload.CardLastFour,
			Id = billing.Id,
			PaymentMethod = payload.PaymentMethod,
			StripePaymentIntentId = payload.StripePaymentIntentId
		});

		return BuildResult(user.Id, user.FirstName, payload.CustomerEmail, product, subscription, payload.CardLastFour);
	}

	public async Task HandleSubscriptionDeletedAsync(SubscriptionDeletedPayload payload)
	{
		var stripeRecord = (await _subscriptionStripeAdapter.FindAsync(
			x => x.StripeSubscriptionId == payload.StripeSubscriptionId)).FirstOrDefault();

		if (stripeRecord is null)
			return;

		var subscription = (await _subscriptionAdapter.GetByIdAsync(stripeRecord.Id))!;

		_currentUserService.Set(subscription.UserId, string.Empty);

		subscription.CancelledDate = DateTime.UtcNow;
		subscription.Status = SubscriptionStatuses.Cancelled;

		await _subscriptionAdapter.UpdateAsync(subscription);
	}

	public async Task HandleSubscriptionUpdatedAsync(SubscriptionUpdatedPayload payload)
	{
		var stripeRecord = (await _subscriptionStripeAdapter.FindAsync(
			x => x.StripeSubscriptionId == payload.StripeSubscriptionId)).FirstOrDefault();

		if (stripeRecord is null)
			return;

		var subscription = (await _subscriptionAdapter.GetByIdAsync(stripeRecord.Id))!;

		_currentUserService.Set(subscription.UserId, string.Empty);

		subscription.EndDate = payload.CurrentPeriodEnd;
		subscription.RenewalDate = payload.CurrentPeriodEnd;

		if (payload.CancelAtPeriodEnd)
		{
			subscription.CancelledDate = DateTime.UtcNow;
			subscription.Status = SubscriptionStatuses.Cancelling;
		}
		else
		{
			subscription.CancelledDate = null;
			subscription.Status = MapStripeStatus(payload.Status);
		}

		await _subscriptionAdapter.UpdateAsync(subscription);

		if (!string.IsNullOrEmpty(payload.StripePriceId) && payload.StripePriceId != stripeRecord.StripePriceId)
		{
			stripeRecord.StripePriceId = payload.StripePriceId;
			await _subscriptionStripeAdapter.UpdateAsync(stripeRecord);

			if (payload.ProductId.HasValue)
			{
				subscription.ProductId = payload.ProductId.Value;
				await _subscriptionAdapter.UpdateAsync(subscription);
			}
		}
	}

	private static CheckoutCompletedResult BuildResult(
		long userId, string firstName, string email, Product product, Subscription subscription, string cardLastFour)
	{
		var planName = product.ProductName.StartsWith("Pro") ? "Pro" : product.ProductName;
		var period = product.ProductName.Contains("Monthly") ? "/mo" : "/yr";

		return new CheckoutCompletedResult
		{
			Amount = $"${product.VendorPrice:F2}{period}",
			CardLastFour = cardLastFour,
			Plan = planName,
			RenewalDate = subscription.RenewalDate.ToString("MMM d, yyyy"),
			UserEmail = email,
			UserFirstName = firstName,
			UserId = userId
		};
	}

	private static string MapStripeStatus(string stripeStatus)
	{
		return stripeStatus switch
		{
			"active" => SubscriptionStatuses.Active,
			"canceled" => SubscriptionStatuses.Cancelled,
			"past_due" => SubscriptionStatuses.PastDue,
			"unpaid" => SubscriptionStatuses.Unpaid,
			_ => stripeStatus
		};
	}


	#endregion
}
