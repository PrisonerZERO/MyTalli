namespace My.Talli.Web.Services.Admin;

using Microsoft.Extensions.Hosting;

/// <summary>Initializer</summary>
public class MaintenanceModeStartupInitializer : IHostedService
{
	#region <Variables>

	private readonly ILogger<MaintenanceModeStartupInitializer> _logger;
	private readonly IMaintenanceModeService _maintenanceModeService;


	#endregion

	#region <Constructors>

	public MaintenanceModeStartupInitializer(ILogger<MaintenanceModeStartupInitializer> logger, IMaintenanceModeService maintenanceModeService)
	{
		_logger = logger;
		_maintenanceModeService = maintenanceModeService;
	}


	#endregion

	#region <Methods>

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		try
		{
			await _maintenanceModeService.RefreshFromDbAsync();
			_logger.LogInformation("MaintenanceModeService cache primed at startup. IsEnabled={IsEnabled}", _maintenanceModeService.IsEnabled);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to prime MaintenanceModeService cache at startup. Defaulting to IsEnabled=false until first AdminHealthWorker tick.");
		}
	}

	public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;


	#endregion
}
