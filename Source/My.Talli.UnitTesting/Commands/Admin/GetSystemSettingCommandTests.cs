namespace My.Talli.UnitTesting.Commands.Admin;

using Domain.Models;
using My.Talli.UnitTesting.Infrastructure.Builders;

/// <summary>Tests</summary>
public class GetSystemSettingCommandTests
{
    #region <Methods>

    [Fact]
    public async Task Execute_KeyNotFound_ReturnsNull()
    {
        var builder = new AdminBuilder();

        var result = await builder.GetSystemSetting.ExecuteAsync("MaintenanceMode");

        Assert.Null(result);
    }

    [Fact]
    public async Task Execute_KeyFound_ReturnsValue()
    {
        var builder = new AdminBuilder();
        await builder.SystemSettingAdapter.InsertAsync(new SystemSetting
        {
            SettingKey = "MaintenanceMode",
            SettingValue = "true"
        });

        var result = await builder.GetSystemSetting.ExecuteAsync("MaintenanceMode");

        Assert.Equal("true", result);
    }

    [Fact]
    public async Task Execute_DifferentKey_ReturnsNullEvenWhenOtherKeysExist()
    {
        var builder = new AdminBuilder();
        await builder.SystemSettingAdapter.InsertAsync(new SystemSetting
        {
            SettingKey = "MaintenanceMode",
            SettingValue = "true"
        });

        var result = await builder.GetSystemSetting.ExecuteAsync("SomeOtherKey");

        Assert.Null(result);
    }

    [Fact]
    public async Task Execute_KeyLookupIsCaseSensitive()
    {
        var builder = new AdminBuilder();
        await builder.SystemSettingAdapter.InsertAsync(new SystemSetting
        {
            SettingKey = "MaintenanceMode",
            SettingValue = "true"
        });

        var result = await builder.GetSystemSetting.ExecuteAsync("maintenancemode");

        Assert.Null(result);
    }

    #endregion
}
