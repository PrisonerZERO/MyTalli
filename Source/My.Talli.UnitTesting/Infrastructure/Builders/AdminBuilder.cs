namespace My.Talli.UnitTesting.Infrastructure.Builders;

using Domain.Commands.Admin;
using Domain.Models;
using Domain.Repositories;
using Lamar;
using My.Talli.UnitTesting.Infrastructure.IoC;
using My.Talli.Web.Services.Admin;

using ENTITIES = Domain.Entities;

/// <summary>Builder</summary>
public class AdminBuilder
{
	#region <Variables>

	private readonly Container _container;

	#endregion

	#region <Properties>

	public IServiceProvider Container => _container;

	public GetSystemSettingCommand GetSystemSetting => _container.GetInstance<GetSystemSettingCommand>();

	public RepositoryAdapterAsync<Heartbeat, ENTITIES.Heartbeat> HeartbeatAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<Heartbeat, ENTITIES.Heartbeat>>();

	public IMaintenanceModeService MaintenanceMode => _container.GetInstance<IMaintenanceModeService>();

	public RepositoryAdapterAsync<SystemSetting, ENTITIES.SystemSetting> SystemSettingAdapter =>
		_container.GetInstance<RepositoryAdapterAsync<SystemSetting, ENTITIES.SystemSetting>>();

	public UpsertSystemSettingCommand UpsertSystemSetting => _container.GetInstance<UpsertSystemSettingCommand>();

	public WriteHeartbeatTickCommand WriteHeartbeatTick => _container.GetInstance<WriteHeartbeatTickCommand>();

	#endregion

	#region <Constructors>

	public AdminBuilder()
	{
		_container = new Container(new ContainerRegistry());
	}

	#endregion
}
