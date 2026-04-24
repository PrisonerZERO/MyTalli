namespace My.Talli.Web.Workers;

using Domain.Commands.Platforms;
using Domain.Data.Interfaces;
using Domain.Models;
using Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Web.Services.Platforms;

using ENTITIES = Domain.Entities;

/// <summary>Worker</summary>
public class TokenRefreshWorker : BackgroundService
{
    #region <Constants>

    private static readonly TimeSpan InitialDelay = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan LoopInterval = TimeSpan.FromHours(6);
    private static readonly TimeSpan PerShopDelay = TimeSpan.FromMilliseconds(250);

    #endregion

    #region <Variables>

    private readonly ILogger<TokenRefreshWorker> _logger;
    private readonly IServiceProvider _serviceProvider;

    #endregion

    #region <Constructors>

    public TokenRefreshWorker(ILogger<TokenRefreshWorker> logger, IServiceProvider serviceProvider)
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token refresh pass threw an unhandled exception.");
            }

            try { await Task.Delay(LoopInterval, stoppingToken); }
            catch (TaskCanceledException) { return; }
        }
    }

    private async Task RunPassAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var sp = scope.ServiceProvider;

        var refreshers = sp.GetServices<IPlatformTokenRefresher>().ToList();
        if (refreshers.Count == 0)
            return;

        var shopAdapter = sp.GetRequiredService<RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection>>();
        var refreshCommand = sp.GetRequiredService<RefreshShopTokensCommand>();
        var currentUserService = sp.GetRequiredService<ICurrentUserService>();

        foreach (var refresher in refreshers)
        {
            if (stoppingToken.IsCancellationRequested)
                return;

            await RefreshPlatformAsync(refresher, shopAdapter, refreshCommand, currentUserService, stoppingToken);
        }
    }

    private async Task RefreshPlatformAsync(IPlatformTokenRefresher refresher, RepositoryAdapterAsync<ShopConnection, ENTITIES.ShopConnection> shopAdapter, RefreshShopTokensCommand refreshCommand, ICurrentUserService currentUserService, CancellationToken stoppingToken)
    {
        var platform = refresher.Platform;
        var threshold = DateTime.UtcNow.Add(refresher.ProactiveRefreshWindow);

        var dueShops = (await shopAdapter.FindAsync(s =>
            s.PlatformConnection.Platform == platform &&
            s.IsEnabled &&
            s.RefreshToken != null &&
            s.RefreshTokenExpiryDateTime != null &&
            s.RefreshTokenExpiryDateTime < threshold)).ToList();

        if (dueShops.Count == 0)
            return;

        _logger.LogInformation("Token refresh: {Count} {Platform} shop(s) due.", dueShops.Count, platform);

        foreach (var shop in dueShops)
        {
            if (stoppingToken.IsCancellationRequested)
                return;

            // Audit fields require an authenticated user — stamp as the shop's owner
            currentUserService.Set(shop.UserId, string.Empty);
            try { await RefreshOneShopAsync(shop, refresher, refreshCommand, stoppingToken); }
            finally { currentUserService.Clear(); }

            try { await Task.Delay(PerShopDelay, stoppingToken); }
            catch (TaskCanceledException) { return; }
        }
    }

    private async Task RefreshOneShopAsync(ShopConnection shop, IPlatformTokenRefresher refresher, RefreshShopTokensCommand refreshCommand, CancellationToken stoppingToken)
    {
        try
        {
            var result = await refresher.RefreshAsync(shop.RefreshToken!, stoppingToken);
            await refreshCommand.ExecuteAsync(shop.Id, result.AccessToken, result.AccessTokenExpiryDateTime, result.RefreshToken, result.RefreshTokenExpiryDateTime);
            _logger.LogInformation("Refreshed {Platform} tokens for shop {ShopId}.", refresher.Platform, shop.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to refresh {Platform} tokens for shop {ShopId}.", refresher.Platform, shop.Id);
            try { await refreshCommand.RecordFailureAsync(shop.Id, $"Token refresh failed: {ex.Message}"); }
            catch (Exception recordEx) { _logger.LogError(recordEx, "Failed to record token refresh error for shop {ShopId}.", shop.Id); }
        }
    }

    #endregion
}
