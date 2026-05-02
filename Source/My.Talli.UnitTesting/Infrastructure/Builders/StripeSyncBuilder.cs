namespace My.Talli.UnitTesting.Infrastructure.Builders;

using Domain.Commands.Platforms;
using Domain.Models;
using Domain.Repositories;
using Lamar;
using My.Talli.UnitTesting.Infrastructure.IoC;
using My.Talli.UnitTesting.Infrastructure.Stubs;
using My.Talli.Web.Services.Platforms;

using ENTITIES = Domain.Entities;

/// <summary>Builder</summary>
public class StripeSyncBuilder
{
    #region <Variables>

    private readonly Container _container;

    #endregion

    #region <Properties>

    public StripeApiClientStub ApiClient { get; } = new();

    public CapturingLogger<StripeConnectSyncService> Logger { get; } = new();

    public RepositoryAdapterAsync<Payout, ENTITIES.Payout> PayoutAdapter =>
        _container.GetInstance<RepositoryAdapterAsync<Payout, ENTITIES.Payout>>();

    public RepositoryAdapterAsync<PayoutStripe, ENTITIES.PayoutStripe> PayoutStripeAdapter =>
        _container.GetInstance<RepositoryAdapterAsync<PayoutStripe, ENTITIES.PayoutStripe>>();

    public RepositoryAdapterAsync<Revenue, ENTITIES.Revenue> RevenueAdapter =>
        _container.GetInstance<RepositoryAdapterAsync<Revenue, ENTITIES.Revenue>>();

    public RepositoryAdapterAsync<RevenueStripe, ENTITIES.RevenueStripe> RevenueStripeAdapter =>
        _container.GetInstance<RepositoryAdapterAsync<RevenueStripe, ENTITIES.RevenueStripe>>();

    public StripeConnectSyncService Service { get; }

    public RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> ShopConnectionAdapter =>
        _container.GetInstance<RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection>>();

    #endregion

    #region <Constructors>

    public StripeSyncBuilder()
    {
        _container = new Container(new ContainerRegistry());

        Service = new StripeConnectSyncService(
            Logger,
            ApiClient,
            _container.GetInstance<UpsertStripePayoutCommand>(),
            _container.GetInstance<UpsertStripeRevenueCommand>());
    }

    #endregion
}
