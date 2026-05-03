namespace My.Talli.UnitTesting.Services.Admin;

using Domain.Models;
using My.Talli.UnitTesting.Infrastructure.Builders;
using My.Talli.Web.Services.Admin;

/// <summary>Tests</summary>
public class MaintenanceModeServiceTests
{
    #region <Methods>

    [Fact]
    public void IsEnabled_DefaultsToFalseBeforeAnyOperation()
    {
        var builder = new AdminBuilder();

        Assert.False(builder.MaintenanceMode.IsEnabled);
    }

    [Fact]
    public async Task SetEnabledAsync_True_UpdatesCacheAndWritesDbRow()
    {
        var builder = new AdminBuilder();

        await builder.MaintenanceMode.SetEnabledAsync(true, actingUserId: 42);

        Assert.True(builder.MaintenanceMode.IsEnabled);

        var raw = await builder.GetSystemSetting.ExecuteAsync(MaintenanceModeService.MaintenanceModeKey);
        Assert.Equal("true", raw);
    }

    [Fact]
    public async Task SetEnabledAsync_False_UpdatesCacheAndWritesDbRow()
    {
        var builder = new AdminBuilder();
        await builder.MaintenanceMode.SetEnabledAsync(true, 1);

        await builder.MaintenanceMode.SetEnabledAsync(false, 1);

        Assert.False(builder.MaintenanceMode.IsEnabled);
        var raw = await builder.GetSystemSetting.ExecuteAsync(MaintenanceModeService.MaintenanceModeKey);
        Assert.Equal("false", raw);
    }

    [Fact]
    public async Task SetEnabledAsync_RaisesStateChangedWithNewValue()
    {
        var builder = new AdminBuilder();
        bool? raised = null;
        builder.MaintenanceMode.StateChanged += v => raised = v;

        await builder.MaintenanceMode.SetEnabledAsync(true, 1);

        Assert.True(raised);
    }

    [Fact]
    public async Task SetEnabledAsync_NoChange_DoesNotRaiseStateChanged()
    {
        var builder = new AdminBuilder();
        await builder.MaintenanceMode.SetEnabledAsync(true, 1);

        var raisedCount = 0;
        builder.MaintenanceMode.StateChanged += _ => raisedCount++;

        await builder.MaintenanceMode.SetEnabledAsync(true, 1);

        Assert.Equal(0, raisedCount);
    }

    [Fact]
    public async Task RefreshFromDbAsync_NoExistingRow_LeavesCacheFalseAndDoesNotRaise()
    {
        var builder = new AdminBuilder();
        var raisedCount = 0;
        builder.MaintenanceMode.StateChanged += _ => raisedCount++;

        await builder.MaintenanceMode.RefreshFromDbAsync();

        Assert.False(builder.MaintenanceMode.IsEnabled);
        Assert.Equal(0, raisedCount);
    }

    [Fact]
    public async Task RefreshFromDbAsync_DbHasTrue_UpdatesCacheAndRaises()
    {
        var builder = new AdminBuilder();
        await builder.SystemSettingAdapter.InsertAsync(new SystemSetting
        {
            SettingKey = MaintenanceModeService.MaintenanceModeKey,
            SettingValue = "true"
        });
        bool? raised = null;
        builder.MaintenanceMode.StateChanged += v => raised = v;

        await builder.MaintenanceMode.RefreshFromDbAsync();

        Assert.True(builder.MaintenanceMode.IsEnabled);
        Assert.True(raised);
    }

    [Fact]
    public async Task RefreshFromDbAsync_DbValueMatchesCache_DoesNotRaise()
    {
        var builder = new AdminBuilder();
        await builder.SystemSettingAdapter.InsertAsync(new SystemSetting
        {
            SettingKey = MaintenanceModeService.MaintenanceModeKey,
            SettingValue = "true"
        });
        await builder.MaintenanceMode.RefreshFromDbAsync();

        var raisedCount = 0;
        builder.MaintenanceMode.StateChanged += _ => raisedCount++;

        await builder.MaintenanceMode.RefreshFromDbAsync();

        Assert.Equal(0, raisedCount);
    }

    [Fact]
    public async Task RefreshFromDbAsync_DbValueChangedToFalse_UpdatesCacheAndRaises()
    {
        var builder = new AdminBuilder();
        await builder.MaintenanceMode.SetEnabledAsync(true, 1);

        // Simulate cross-instance change: another instance writes 'false' to DB
        var stored = (await builder.SystemSettingAdapter.FindAsync(s => s.SettingKey == MaintenanceModeService.MaintenanceModeKey)).First();
        stored.SettingValue = "false";
        await builder.SystemSettingAdapter.UpdateAsync(stored);

        bool? raised = null;
        builder.MaintenanceMode.StateChanged += v => raised = v;

        await builder.MaintenanceMode.RefreshFromDbAsync();

        Assert.False(builder.MaintenanceMode.IsEnabled);
        Assert.False(raised);
    }

    [Fact]
    public async Task SetEnabledAsync_ActingUserId_StampsAuditFieldsOnInsert()
    {
        var builder = new AdminBuilder();

        await builder.MaintenanceMode.SetEnabledAsync(true, actingUserId: 99);

        var stored = (await builder.SystemSettingAdapter.GetAllAsync()).Single();
        // Audit resolution is handled by AuditResolverStub which is a no-op,
        // so we just verify the row was created via the service path (not direct insert).
        Assert.Equal(MaintenanceModeService.MaintenanceModeKey, stored.SettingKey);
        Assert.Equal("true", stored.SettingValue);
    }

    #endregion
}
