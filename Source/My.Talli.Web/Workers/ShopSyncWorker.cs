namespace My.Talli.Web.Workers;

using Domain.Commands.Admin;
using Domain.Commands.Platforms;
using Domain.Data.Interfaces;
using Domain.Models;
using Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Web.Services.Platforms;

using ENTITIES = Domain.Entities;

/// <summary>Worker</summary>
public class ShopSyncWorker : BackgroundService
{
    #region <Constants>

    public const string HeartbeatSourceName = "ShopSyncWorker";

    private const int ErrorMessageMaxLength = 1900;
    private const int HeartbeatExpectedIntervalSeconds = 300;
    private const int MaxShopsPerPass = 50;
    private static readonly TimeSpan FailureBaseBackoff = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan FailureMaxBackoff = TimeSpan.FromHours(24);
    private static readonly TimeSpan InitialDelay = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan LoopInterval = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan PerShopDelay = TimeSpan.FromMilliseconds(500);
    private static readonly TimeSpan SuccessNextSyncInterval = TimeSpan.FromHours(24);

    #endregion

    #region <Variables>

    private readonly ILogger<ShopSyncWorker> _logger;
    private readonly IServiceProvider _serviceProvider;

    #endregion

    #region <Constructors>

    public ShopSyncWorker(ILogger<ShopSyncWorker> logger, IServiceProvider serviceProvider)
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
                await RunPassAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Shop sync pass threw an unhandled exception.");
            }

            try { await Task.Delay(LoopInterval, stoppingToken); }
            catch (TaskCanceledException) { return; }
        }
    }

    private async Task RunPassAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var sp = scope.ServiceProvider;

        var syncServices = sp.GetServices<IPlatformSyncService>().ToList();
        if (syncServices.Count == 0)
        {
            await WriteHeartbeatAsync(sp);
            return;
        }

        var shopAdapter = sp.GetRequiredService<RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection>>();
        var stateCommand = sp.GetRequiredService<UpdateShopSyncStateCommand>();
        var currentUserService = sp.GetRequiredService<ICurrentUserService>();

        foreach (var syncService in syncServices)
        {
            if (stoppingToken.IsCancellationRequested)
                return;

            await SyncPlatformAsync(syncService, shopAdapter, stateCommand, currentUserService, stoppingToken);
        }

        await WriteHeartbeatAsync(sp);
    }

    private async Task WriteHeartbeatAsync(IServiceProvider scopedServices)
    {
        try
        {
            var writeHeartbeat = scopedServices.GetRequiredService<WriteHeartbeatTickCommand>();
            var currentUser = scopedServices.GetRequiredService<ICurrentUserService>();

            currentUser.Set(0L, string.Empty);
            try
            {
                await writeHeartbeat.ExecuteAsync(HeartbeatSourceName, HeartbeatExpectedIntervalSeconds);
            }
            finally
            {
                currentUser.Clear();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ShopSyncWorker heartbeat write failed.");
        }
    }

    private async Task SyncPlatformAsync(IPlatformSyncService syncService, RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> shopAdapter, UpdateShopSyncStateCommand stateCommand, ICurrentUserService currentUserService, CancellationToken stoppingToken)
    {
        var platform = syncService.Platform;
        var now = DateTime.UtcNow;

        var dueShops = (await shopAdapter.FindAsync(s =>
            s.PlatformConnection.Platform == platform &&
            s.IsActive &&
            s.IsEnabled &&
            s.NextSyncDateTime <= now))
            .OrderBy(s => s.NextSyncDateTime)
            .Take(MaxShopsPerPass)
            .ToList();

        if (dueShops.Count == 0)
            return;

        _logger.LogInformation("Sync: {Count} {Platform} shop(s) due.", dueShops.Count, platform);

        foreach (var shop in dueShops)
        {
            if (stoppingToken.IsCancellationRequested)
                return;

            // Audit fields require an authenticated user — stamp as the shop's owner
            currentUserService.Set(shop.UserId, string.Empty);
            try { await SyncOneShopAsync(shop, syncService, stateCommand, stoppingToken); }
            finally { currentUserService.Clear(); }

            try { await Task.Delay(PerShopDelay, stoppingToken); }
            catch (TaskCanceledException) { return; }
        }
    }

    private async Task SyncOneShopAsync(ShopConnection shop, IPlatformSyncService syncService, UpdateShopSyncStateCommand stateCommand, CancellationToken stoppingToken)
    {
        try
        {
            await stateCommand.MarkInProgressAsync(shop.Id);

            var result = await syncService.SyncShopAsync(shop, stoppingToken);

            var syncedAt = DateTime.UtcNow;
            var nextSyncAt = syncedAt.Add(SuccessNextSyncInterval);
            await stateCommand.MarkCompletedAsync(shop.Id, syncedAt, nextSyncAt);

            _logger.LogInformation("{Platform} shop {ShopId} synced: {NewRows} new row(s) across {Pages} page(s).", syncService.Platform, shop.Id, result.NewRevenueRowCount, result.PagesFetched);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "{Platform} sync failed for shop {ShopId}.", syncService.Platform, shop.Id);

            var failureCount = shop.ConsecutiveFailures + 1;
            var nextSyncAt = DateTime.UtcNow.Add(ComputeBackoff(failureCount));
            var message = Truncate(ex.Message, ErrorMessageMaxLength);

            try { await stateCommand.MarkFailedAsync(shop.Id, message, nextSyncAt); }
            catch (Exception recordEx) { _logger.LogError(recordEx, "Failed to record sync failure for shop {ShopId}.", shop.Id); }
        }
    }

    private static TimeSpan ComputeBackoff(int consecutiveFailures)
    {
        var exponent = Math.Min(consecutiveFailures, 10);
        var backoff = TimeSpan.FromTicks(FailureBaseBackoff.Ticks * (long)Math.Pow(2, exponent));
        return backoff > FailureMaxBackoff ? FailureMaxBackoff : backoff;
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            return value;

        return value[..maxLength];
    }

    #endregion
}
