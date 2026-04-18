namespace My.Talli.UnitTesting.Infrastructure.Builders;

using Domain.Commands.Platforms;
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

	public RepositoryAdapterAsync<PlatformConnection, ENTITIES.PlatformConnection> PlatformConnectionAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<PlatformConnection, ENTITIES.PlatformConnection>>();

	public RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> ShopConnectionAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection>>();

	public RepositoryAdapterAsync<ShopConnectionEtsy, ENTITIES.ShopConnectionEtsy> ShopConnectionEtsyAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<ShopConnectionEtsy, ENTITIES.ShopConnectionEtsy>>();

	#endregion

	#region <Constructors>

	public PlatformHandlerBuilder()
	{
		_container = new Container(new ContainerRegistry());
	}

	#endregion
}
