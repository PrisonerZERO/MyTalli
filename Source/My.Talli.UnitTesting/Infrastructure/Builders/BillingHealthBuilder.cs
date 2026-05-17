namespace My.Talli.UnitTesting.Infrastructure.Builders;

using Domain.Models;
using Domain.Repositories;
using Lamar;
using Microsoft.Extensions.Logging;
using My.Talli.UnitTesting.Infrastructure.IoC;
using My.Talli.UnitTesting.Infrastructure.Stubs;
using My.Talli.Web.Commands.Billing;

using ENTITIES = My.Talli.Domain.Entities;

/// <summary>Builder</summary>
public class BillingHealthBuilder
{
	#region <Variables>

	private readonly Container _container;

	#endregion

	#region <Constructors>

	public BillingHealthBuilder()
	{
		_container = new Container(new ContainerRegistry());
	}

	#endregion

	#region <Properties>

	public StripeBillingApiClientStub ApiClient => (StripeBillingApiClientStub)_container.GetInstance<My.Talli.Web.Services.Billing.IStripeBillingApiClient>();

	public IServiceProvider Container => _container;

	public RepositoryAdapterAsync<Heartbeat, ENTITIES.Heartbeat> HeartbeatAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<Heartbeat, ENTITIES.Heartbeat>>();

	public CapturingLogger<ReconcileBillingHealthCommand> ReconcileLogger =>
		(CapturingLogger<ReconcileBillingHealthCommand>)_container.GetInstance<ILogger<ReconcileBillingHealthCommand>>();

	public RepositoryAdapterAsync<Subscription, ENTITIES.Subscription> SubscriptionAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<Subscription, ENTITIES.Subscription>>();

	public RepositoryAdapterAsync<SubscriptionStripe, ENTITIES.SubscriptionStripe> SubscriptionStripeAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<SubscriptionStripe, ENTITIES.SubscriptionStripe>>();

	#endregion

	#region <Methods>

	/// <summary>Seeds a Subscription + SubscriptionStripe pair sharing the same PK; returns the inserted Subscription.</summary>
	public async Task<Subscription> SeedActiveProAsync(long userId, string stripeSubscriptionId, DateTime endDate, string status = "Active")
	{
		var sub = await SubscriptionAdapter.InsertAsync(new Subscription
		{
			EndDate = endDate,
			OrderItemId = 0,
			ProductId = 1,
			RenewalDate = endDate,
			StartDate = endDate.AddMonths(-1),
			Status = status,
			UserId = userId
		});

		await SubscriptionStripeAdapter.InsertAsync(new SubscriptionStripe
		{
			Id = sub.Id,
			StripeCustomerId = $"cus_test_{sub.Id}",
			StripePriceId = "price_test",
			StripeSubscriptionId = stripeSubscriptionId
		});

		return sub;
	}

	#endregion
}
