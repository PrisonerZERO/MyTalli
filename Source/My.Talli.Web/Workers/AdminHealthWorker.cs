namespace My.Talli.Web.Workers;

using Domain.Commands.Admin;
using Domain.Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Web.Services.Admin;

/// <summary>Worker</summary>
public class AdminHealthWorker : BackgroundService
{
	#region <Constants>

	public const string HeartbeatSourceName = "AdminHealthWorker";
	public const int ExpectedIntervalSeconds = 60;

	private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(15);
	private static readonly TimeSpan LoopInterval = TimeSpan.FromMinutes(1);


	#endregion

	#region <Variables>

	private readonly ILogger<AdminHealthWorker> _logger;
	private readonly IServiceProvider _serviceProvider;


	#endregion

	#region <Constructors>

	public AdminHealthWorker(ILogger<AdminHealthWorker> logger, IServiceProvider serviceProvider)
	{
		_logger = logger;
		_serviceProvider = serviceProvider;
	}


	#endregion

	#region <Methods>

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		try { await Task.Delay(InitialDelay, stoppingToken); }
		catch (TaskCanceledException) { return; }

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				using var scope = _serviceProvider.CreateScope();
				await RunTickAsync(scope.ServiceProvider, _logger, stoppingToken);
			}
			catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
			{
				return;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "AdminHealthWorker tick threw an unhandled exception.");
			}

			try { await Task.Delay(LoopInterval, stoppingToken); }
			catch (TaskCanceledException) { return; }
		}
	}

	public static async Task RunTickAsync(IServiceProvider scopedServices, ILogger logger, CancellationToken cancellationToken)
	{
		var maintenanceMode = scopedServices.GetRequiredService<IMaintenanceModeService>();
		var writeHeartbeat = scopedServices.GetRequiredService<WriteHeartbeatTickCommand>();
		var currentUser = scopedServices.GetRequiredService<ICurrentUserService>();

		currentUser.Set(0L, string.Empty);
		try
		{
			await maintenanceMode.RefreshFromDbAsync();

			if (cancellationToken.IsCancellationRequested) return;

			await writeHeartbeat.ExecuteAsync(HeartbeatSourceName, ExpectedIntervalSeconds);

			logger.LogDebug("AdminHealthWorker tick complete. MaintenanceMode={IsEnabled}", maintenanceMode.IsEnabled);
		}
		finally
		{
			currentUser.Clear();
		}
	}


	#endregion
}
