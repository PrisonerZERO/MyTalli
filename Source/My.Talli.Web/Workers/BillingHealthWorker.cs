namespace My.Talli.Web.Workers;

using Domain.Commands.Admin;
using Domain.Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Web.Commands.Billing;

/// <summary>Worker</summary>
public class BillingHealthWorker : BackgroundService
{
	#region <Constants>

	public const string HeartbeatSourceName = "BillingHealthWorker";
	public const int ExpectedIntervalSeconds = 86400;

	private static readonly TimeSpan InitialDelay = TimeSpan.FromMinutes(2);
	private static readonly TimeSpan LoopInterval = TimeSpan.FromHours(24);
	private static readonly TimeSpan PerSubscriptionDelay = TimeSpan.FromMilliseconds(250);

	#endregion

	#region <Variables>

	private readonly ILogger<BillingHealthWorker> _logger;
	private readonly IServiceProvider _serviceProvider;

	#endregion

	#region <Constructors>

	public BillingHealthWorker(ILogger<BillingHealthWorker> logger, IServiceProvider serviceProvider)
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
				await RunPassAsync(scope.ServiceProvider, _logger, PerSubscriptionDelay, stoppingToken);
			}
			catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
			{
				return;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "BillingHealthWorker pass threw an unhandled exception.");
			}

			try { await Task.Delay(LoopInterval, stoppingToken); }
			catch (TaskCanceledException) { return; }
		}
	}

	public static async Task<int> RunPassAsync(IServiceProvider scopedServices, ILogger logger, TimeSpan perSubscriptionDelay, CancellationToken cancellationToken)
	{
		var reconcile = scopedServices.GetRequiredService<ReconcileBillingHealthCommand>();
		var writeHeartbeat = scopedServices.GetRequiredService<WriteHeartbeatTickCommand>();
		var currentUser = scopedServices.GetRequiredService<ICurrentUserService>();

		currentUser.Set(0L, string.Empty);
		try
		{
			var driftCount = await reconcile.ExecuteAsync(perSubscriptionDelay, cancellationToken);

			if (driftCount > 0)
				logger.LogWarning("BillingHealthWorker pass complete: {DriftCount} subscription(s) with drift.", driftCount);
			else
				logger.LogInformation("BillingHealthWorker pass complete: no drift detected.");

			try
			{
				await writeHeartbeat.ExecuteAsync(HeartbeatSourceName, ExpectedIntervalSeconds);
			}
			catch (Exception ex)
			{
				logger.LogWarning(ex, "BillingHealthWorker heartbeat write failed.");
			}

			return driftCount;
		}
		finally
		{
			currentUser.Clear();
		}
	}

	#endregion
}
