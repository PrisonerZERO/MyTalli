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
public class EtsySyncBuilder
{
    #region <Variables>

    private readonly Container _container;

    #endregion

    #region <Properties>

    public EtsyApiClientStub ApiClient { get; } = new();

    public RepositoryAdapterAsync<Expense, ENTITIES.Expense> ExpenseAdapter =>
        _container.GetInstance<RepositoryAdapterAsync<Expense, ENTITIES.Expense>>();

    public RepositoryAdapterAsync<ExpenseEtsy, ENTITIES.ExpenseEtsy> ExpenseEtsyAdapter =>
        _container.GetInstance<RepositoryAdapterAsync<ExpenseEtsy, ENTITIES.ExpenseEtsy>>();

    public CapturingLogger<EtsySyncService> Logger { get; } = new();

    public RepositoryAdapterAsync<Payout, ENTITIES.Payout> PayoutAdapter =>
        _container.GetInstance<RepositoryAdapterAsync<Payout, ENTITIES.Payout>>();

    public RepositoryAdapterAsync<PayoutEtsy, ENTITIES.PayoutEtsy> PayoutEtsyAdapter =>
        _container.GetInstance<RepositoryAdapterAsync<PayoutEtsy, ENTITIES.PayoutEtsy>>();

    public RepositoryAdapterAsync<Revenue, ENTITIES.Revenue> RevenueAdapter =>
        _container.GetInstance<RepositoryAdapterAsync<Revenue, ENTITIES.Revenue>>();

    public RepositoryAdapterAsync<RevenueEtsy, ENTITIES.RevenueEtsy> RevenueEtsyAdapter =>
        _container.GetInstance<RepositoryAdapterAsync<RevenueEtsy, ENTITIES.RevenueEtsy>>();

    public EtsySyncService Service { get; }

    public RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> ShopConnectionAdapter =>
        _container.GetInstance<RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection>>();

    public IShopTokenProtector TokenProtector => _container.GetInstance<IShopTokenProtector>();

    public EtsyTokenRefresher TokenRefresher { get; }

    #endregion

    #region <Constructors>

    public EtsySyncBuilder()
    {
        _container = new Container(new ContainerRegistry());

        TokenRefresher = new EtsyTokenRefresher(ApiClient);

        Service = new EtsySyncService(
            ApiClient,
            TokenRefresher,
            Logger,
            _container.GetInstance<RefreshShopTokensCommand>(),
            _container.GetInstance<IShopTokenProtector>(),
            _container.GetInstance<UpsertEtsyExpenseCommand>(),
            _container.GetInstance<UpsertEtsyPayoutCommand>(),
            _container.GetInstance<UpsertEtsyRevenueCommand>());
    }

    #endregion
}
