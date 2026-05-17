namespace My.Talli.UnitTesting.Infrastructure.Builders;

using Domain.Commands.Billing;
using Domain.Models;
using Domain.Repositories;
using Lamar;
using Microsoft.Extensions.Logging;
using My.Talli.UnitTesting.Infrastructure.IoC;
using My.Talli.UnitTesting.Infrastructure.Stubs;
using My.Talli.Web.Commands.Billing;
using My.Talli.Web.Commands.Notifications;

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

	public AcknowledgeExpirationCommand AcknowledgeExpiration => _container.GetInstance<AcknowledgeExpirationCommand>();

	public StripeBillingApiClientStub ApiClient => (StripeBillingApiClientStub)_container.GetInstance<My.Talli.Web.Services.Billing.IStripeBillingApiClient>();

	public CanConnectAnotherShopCommand CanConnectAnotherShop => _container.GetInstance<CanConnectAnotherShopCommand>();

	public GetFreeTierSlotShopIdCommand GetFreeTierSlotShopId => _container.GetInstance<GetFreeTierSlotShopIdCommand>();

	public IServiceProvider Container => _container;

	public EmailServiceStub EmailService => (EmailServiceStub)_container.GetInstance<My.Talli.Web.Services.Email.IEmailService>();

	public FindExpiredUnacknowledgedSubscriptionCommand FindExpiredUnacknowledged => _container.GetInstance<FindExpiredUnacknowledgedSubscriptionCommand>();

	public RepositoryAdapterAsync<UserAuthenticationGoogle, ENTITIES.UserAuthenticationGoogle> GoogleAuthAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<UserAuthenticationGoogle, ENTITIES.UserAuthenticationGoogle>>();

	public RepositoryAdapterAsync<Heartbeat, ENTITIES.Heartbeat> HeartbeatAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<Heartbeat, ENTITIES.Heartbeat>>();

	public RepositoryAdapterAsync<PlatformConnection, ENTITIES.PlatformConnection> PlatformConnectionAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<PlatformConnection, ENTITIES.PlatformConnection>>();

	public RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> ShopConnectionAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection>>();

	public CapturingLogger<NotifyExpiredSubscribersCommand> NotifyLogger =>
		(CapturingLogger<NotifyExpiredSubscribersCommand>)_container.GetInstance<ILogger<NotifyExpiredSubscribersCommand>>();

	public NotifyExpiredSubscribersCommand NotifyExpiredSubscribers => _container.GetInstance<NotifyExpiredSubscribersCommand>();

	public CapturingLogger<ReconcileBillingHealthCommand> ReconcileLogger =>
		(CapturingLogger<ReconcileBillingHealthCommand>)_container.GetInstance<ILogger<ReconcileBillingHealthCommand>>();

	public RepositoryAdapterAsync<Subscription, ENTITIES.Subscription> SubscriptionAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<Subscription, ENTITIES.Subscription>>();

	public RepositoryAdapterAsync<SubscriptionStripe, ENTITIES.SubscriptionStripe> SubscriptionStripeAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<SubscriptionStripe, ENTITIES.SubscriptionStripe>>();

	public RepositoryAdapterAsync<User, ENTITIES.User> UserAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<User, ENTITIES.User>>();

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

	/// <summary>Seeds a PlatformConnection + ShopConnection pair for the given user on the given platform; returns the shop.</summary>
	public async Task<ShopConnection> SeedShopAsync(long userId, string platform, string shopName = "Test Shop")
	{
		var connection = await PlatformConnectionAdapter.InsertAsync(new PlatformConnection
		{
			ConnectionStatus = "Active",
			Platform = platform,
			UserId = userId
		});

		return await ShopConnectionAdapter.InsertAsync(new ShopConnection
		{
			AccessToken = string.Empty,
			ConsecutiveFailures = 0,
			IsActive = true,
			IsEnabled = true,
			PlatformAccountId = $"acct_{Guid.NewGuid():N}",
			PlatformConnectionId = connection.Id,
			PlatformShopId = $"shop_{Guid.NewGuid():N}",
			ShopName = shopName,
			Status = "Completed",
			UserId = userId
		});
	}

	/// <summary>Seeds a User + UserAuthenticationGoogle pair sharing the same PK; returns the User Id.</summary>
	public async Task<long> SeedGoogleUserAsync(string firstName, string email)
	{
		var user = await UserAdapter.InsertAsync(new User
		{
			DisplayName = firstName,
			FirstName = firstName,
			LastName = string.Empty,
			InitialProvider = "Google",
			PreferredProvider = "Google",
			UserPreferences = "{}"
		});

		await GoogleAuthAdapter.InsertAsync(new UserAuthenticationGoogle
		{
			Id = user.Id,
			AvatarUrl = string.Empty,
			DisplayName = firstName,
			Email = email,
			EmailVerified = true,
			FirstName = firstName,
			GoogleId = $"google_{user.Id}",
			LastName = string.Empty,
			Locale = "en"
		});

		return user.Id;
	}

	#endregion
}
