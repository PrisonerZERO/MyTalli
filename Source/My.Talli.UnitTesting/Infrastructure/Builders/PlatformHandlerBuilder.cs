namespace My.Talli.UnitTesting.Infrastructure.Builders;

using Domain.Commands.Platforms;
using Domain.Components.Tokens;
using Domain.Models;
using Domain.Repositories;
using Lamar;
using My.Talli.UnitTesting.Infrastructure.IoC;

using ENTITIES = Domain.Entities;

/// <summary>Builder</summary>
public class PlatformHandlerBuilder
{
	#region <Variables>

	private readonly Container _container;

	#endregion

	#region <Properties>

	public ConnectEtsyCommand Command => _container.GetInstance<ConnectEtsyCommand>();

	public ConnectGumroadCommand GumroadCommand => _container.GetInstance<ConnectGumroadCommand>();

	public RepositoryAdapterAsync<RevenueGumroad, ENTITIES.RevenueGumroad> RevenueGumroadAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<RevenueGumroad, ENTITIES.RevenueGumroad>>();

	public UpsertGumroadRevenueCommand UpsertGumroadRevenue => _container.GetInstance<UpsertGumroadRevenueCommand>();

	public ConnectStripeCommand StripeCommand => _container.GetInstance<ConnectStripeCommand>();

	public RepositoryAdapterAsync<RevenueStripe, ENTITIES.RevenueStripe> RevenueStripeAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<RevenueStripe, ENTITIES.RevenueStripe>>();

	public RepositoryAdapterAsync<PayoutStripe, ENTITIES.PayoutStripe> PayoutStripeAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<PayoutStripe, ENTITIES.PayoutStripe>>();

	public UpsertStripePayoutCommand UpsertStripePayout => _container.GetInstance<UpsertStripePayoutCommand>();

	public UpsertStripeRevenueCommand UpsertStripeRevenue => _container.GetInstance<UpsertStripeRevenueCommand>();

	public RepositoryAdapterAsync<Expense, ENTITIES.Expense> ExpenseAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<Expense, ENTITIES.Expense>>();

	public RepositoryAdapterAsync<ExpenseEtsy, ENTITIES.ExpenseEtsy> ExpenseEtsyAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<ExpenseEtsy, ENTITIES.ExpenseEtsy>>();

	public RepositoryAdapterAsync<Payout, ENTITIES.Payout> PayoutAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<Payout, ENTITIES.Payout>>();

	public RepositoryAdapterAsync<PayoutEtsy, ENTITIES.PayoutEtsy> PayoutEtsyAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<PayoutEtsy, ENTITIES.PayoutEtsy>>();

	public RepositoryAdapterAsync<PlatformConnection, ENTITIES.PlatformConnection> PlatformConnectionAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<PlatformConnection, ENTITIES.PlatformConnection>>();

	public RepositoryAdapterAsync<Revenue, ENTITIES.Revenue> RevenueAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<Revenue, ENTITIES.Revenue>>();

	public RepositoryAdapterAsync<RevenueEtsy, ENTITIES.RevenueEtsy> RevenueEtsyAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<RevenueEtsy, ENTITIES.RevenueEtsy>>();

	public RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> ShopConnectionAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection>>();

	public RepositoryAdapterAsync<ShopConnectionEtsy, ENTITIES.ShopConnectionEtsy> ShopConnectionEtsyAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<ShopConnectionEtsy, ENTITIES.ShopConnectionEtsy>>();

	public IShopTokenProtector TokenProtector => _container.GetInstance<IShopTokenProtector>();

	public UpsertEtsyExpenseCommand UpsertExpense => _container.GetInstance<UpsertEtsyExpenseCommand>();

	public UpsertEtsyPayoutCommand UpsertPayout => _container.GetInstance<UpsertEtsyPayoutCommand>();

	public UpsertEtsyRevenueCommand UpsertRevenue => _container.GetInstance<UpsertEtsyRevenueCommand>();

	#endregion

	#region <Constructors>

	public PlatformHandlerBuilder()
	{
		_container = new Container(new ContainerRegistry());
	}

	#endregion
}
