namespace My.Talli.UnitTesting.Infrastructure.Builders;

using Domain.Commands.Platforms;
using Domain.Components.Tokens;
using Domain.Models;
using Domain.Repositories;
using Lamar;
using My.Talli.UnitTesting.Infrastructure.IoC;
using My.Talli.UnitTesting.Infrastructure.Stubs;
using My.Talli.Web.Services.Platforms;

using ENTITIES = Domain.Entities;

/// <summary>Builder</summary>
public class GumroadSyncBuilder
{
    #region <Variables>

    private readonly Container _container;

    #endregion

    #region <Properties>

    public GumroadApiClientStub ApiClient { get; } = new();

    public CapturingLogger<GumroadSyncService> Logger { get; } = new();

    public RepositoryAdapterAsync<Revenue, ENTITIES.Revenue> RevenueAdapter =>
        _container.GetInstance<RepositoryAdapterAsync<Revenue, ENTITIES.Revenue>>();

    public RepositoryAdapterAsync<RevenueGumroad, ENTITIES.RevenueGumroad> RevenueGumroadAdapter =>
        _container.GetInstance<RepositoryAdapterAsync<RevenueGumroad, ENTITIES.RevenueGumroad>>();

    public GumroadSyncService Service { get; }

    public RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> ShopConnectionAdapter =>
        _container.GetInstance<RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection>>();

    public IShopTokenProtector TokenProtector => _container.GetInstance<IShopTokenProtector>();

    #endregion

    #region <Constructors>

    public GumroadSyncBuilder()
    {
        _container = new Container(new ContainerRegistry());

        Service = new GumroadSyncService(
            ApiClient,
            Logger,
            _container.GetInstance<IShopTokenProtector>(),
            _container.GetInstance<UpsertGumroadRevenueCommand>());
    }

    #endregion
}
