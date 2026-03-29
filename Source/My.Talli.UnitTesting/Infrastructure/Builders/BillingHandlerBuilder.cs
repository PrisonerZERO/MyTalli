namespace My.Talli.UnitTesting.Infrastructure.Builders;

using Domain.Data.Interfaces;
using Domain.Handlers.Authentication;
using Domain.Handlers.Billing;
using Domain.Models;
using Domain.Repositories;
using Lamar;
using My.Talli.UnitTesting.Infrastructure.IoC;
using My.Talli.UnitTesting.Infrastructure.Stubs;

using ENTITIES = Domain.Entities;

/// <summary>Builder</summary>
public class BillingHandlerBuilder
{
	#region <Variables>

	private readonly Container _container;

	#endregion

	#region <Properties>

	public RepositoryAdapterAsync<Billing, ENTITIES.Billing> BillingAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<Billing, ENTITIES.Billing>>();

	public RepositoryAdapterAsync<BillingStripe, ENTITIES.BillingStripe> BillingStripeAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<BillingStripe, ENTITIES.BillingStripe>>();

	public CurrentUserServiceStub CurrentUserService => (CurrentUserServiceStub)_container.GetInstance<ICurrentUserService>();

	public GoogleSignInHandler GoogleHandler => _container.GetInstance<GoogleSignInHandler>();

	public StripeWebhookHandler Handler => _container.GetInstance<StripeWebhookHandler>();

	public RepositoryAdapterAsync<Order, ENTITIES.Order> OrderAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<Order, ENTITIES.Order>>();

	public RepositoryAdapterAsync<OrderItem, ENTITIES.OrderItem> OrderItemAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<OrderItem, ENTITIES.OrderItem>>();

	public RepositoryAdapterAsync<Product, ENTITIES.Product> ProductAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<Product, ENTITIES.Product>>();

	public RepositoryAdapterAsync<Subscription, ENTITIES.Subscription> SubscriptionAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<Subscription, ENTITIES.Subscription>>();

	public RepositoryAdapterAsync<SubscriptionStripe, ENTITIES.SubscriptionStripe> SubscriptionStripeAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<SubscriptionStripe, ENTITIES.SubscriptionStripe>>();

	public RepositoryAdapterAsync<User, ENTITIES.User> UserAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<User, ENTITIES.User>>();

	#endregion

	#region <Constructors>

	public BillingHandlerBuilder()
	{
		_container = new Container(new ContainerRegistry());
	}

	#endregion
}
