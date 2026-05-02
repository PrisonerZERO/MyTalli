namespace My.Talli.UnitTesting.Workers;

using Domain.Models;
using Microsoft.Extensions.Logging.Abstractions;
using My.Talli.UnitTesting.Infrastructure.Builders;
using My.Talli.Web.Services.Admin;
using My.Talli.Web.Workers;

/// <summary>Tests</summary>
public class AdminHealthWorkerTests
{
    #region <Methods>

    [Fact]
    public async Task RunTickAsync_NoMaintenanceModeRow_LeavesCacheFalseAndWritesHeartbeat()
    {
        var builder = new AdminBuilder();

        await AdminHealthWorker.RunTickAsync(builder.Container, NullLogger.Instance, CancellationToken.None);

        Assert.False(builder.MaintenanceMode.IsEnabled);

        var heartbeat = (await builder.HeartbeatAdapter.GetAllAsync()).Single();
        Assert.Equal(AdminHealthWorker.HeartbeatSourceName, heartbeat.HeartbeatSource);
        Assert.Equal(AdminHealthWorker.ExpectedIntervalSeconds, heartbeat.ExpectedIntervalSeconds);
    }

    [Fact]
    public async Task RunTickAsync_DbHasTrue_PrimesCacheAndWritesHeartbeat()
    {
        var builder = new AdminBuilder();
        await builder.SystemSettingAdapter.InsertAsync(new SystemSetting
        {
            SettingKey = MaintenanceModeService.MaintenanceModeKey,
            SettingValue = "true"
        });

        await AdminHealthWorker.RunTickAsync(builder.Container, NullLogger.Instance, CancellationToken.None);

        Assert.True(builder.MaintenanceMode.IsEnabled);
        Assert.Single(await builder.HeartbeatAdapter.GetAllAsync());
    }

    [Fact]
    public async Task RunTickAsync_CacheStaleVsDb_RaisesStateChangedOnTick()
    {
        var builder = new AdminBuilder();

        // DB says true, cache is still false
        await builder.SystemSettingAdapter.InsertAsync(new SystemSetting
        {
            SettingKey = MaintenanceModeService.MaintenanceModeKey,
            SettingValue = "true"
        });

        bool? raised = null;
        builder.MaintenanceMode.StateChanged += v => raised = v;

        await AdminHealthWorker.RunTickAsync(builder.Container, NullLogger.Instance, CancellationToken.None);

        Assert.True(raised);
    }

    [Fact]
    public async Task RunTickAsync_ConsecutiveTicks_UpdateLastTickAtOnSameRow()
    {
        var builder = new AdminBuilder();

        await AdminHealthWorker.RunTickAsync(builder.Container, NullLogger.Instance, CancellationToken.None);
        var firstTickAt = (await builder.HeartbeatAdapter.GetAllAsync()).Single().LastTickAt;

        await Task.Delay(10);
        await AdminHealthWorker.RunTickAsync(builder.Container, NullLogger.Instance, CancellationToken.None);

        var heartbeats = (await builder.HeartbeatAdapter.GetAllAsync()).ToList();
        Assert.Single(heartbeats);
        Assert.True(heartbeats[0].LastTickAt >= firstTickAt);
    }

    [Fact]
    public async Task RunTickAsync_DbValueUnchanged_DoesNotRaiseStateChanged()
    {
        var builder = new AdminBuilder();
        await AdminHealthWorker.RunTickAsync(builder.Container, NullLogger.Instance, CancellationToken.None);

        var raisedCount = 0;
        builder.MaintenanceMode.StateChanged += _ => raisedCount++;

        await AdminHealthWorker.RunTickAsync(builder.Container, NullLogger.Instance, CancellationToken.None);

        Assert.Equal(0, raisedCount);
    }

    #endregion
}
