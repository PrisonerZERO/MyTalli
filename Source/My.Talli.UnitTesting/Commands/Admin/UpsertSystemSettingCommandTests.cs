namespace My.Talli.UnitTesting.Commands.Admin;

using My.Talli.UnitTesting.Infrastructure.Builders;

/// <summary>Tests</summary>
public class UpsertSystemSettingCommandTests
{
    #region <Methods>

    [Fact]
    public async Task Execute_NewKey_InsertsRow()
    {
        var builder = new AdminBuilder();

        var result = await builder.UpsertSystemSetting.ExecuteAsync("MaintenanceMode", "true");

        Assert.Equal("MaintenanceMode", result.SettingKey);
        Assert.Equal("true", result.SettingValue);
        Assert.NotEqual(0, result.Id);
        Assert.Single(await builder.SystemSettingAdapter.GetAllAsync());
    }

    [Fact]
    public async Task Execute_ExistingKey_UpdatesValueWithoutInserting()
    {
        var builder = new AdminBuilder();
        await builder.UpsertSystemSetting.ExecuteAsync("MaintenanceMode", "false");

        var result = await builder.UpsertSystemSetting.ExecuteAsync("MaintenanceMode", "true");

        Assert.Equal("true", result.SettingValue);
        Assert.Single(await builder.SystemSettingAdapter.GetAllAsync());
    }

    [Fact]
    public async Task Execute_TwoDifferentKeys_InsertsTwoRows()
    {
        var builder = new AdminBuilder();

        await builder.UpsertSystemSetting.ExecuteAsync("MaintenanceMode", "true");
        await builder.UpsertSystemSetting.ExecuteAsync("ReadOnlyMode", "false");

        var all = (await builder.SystemSettingAdapter.GetAllAsync()).ToList();
        Assert.Equal(2, all.Count);
    }

    [Fact]
    public async Task Execute_ExistingKey_PreservesIdAcrossUpdates()
    {
        var builder = new AdminBuilder();
        var first = await builder.UpsertSystemSetting.ExecuteAsync("MaintenanceMode", "false");

        var second = await builder.UpsertSystemSetting.ExecuteAsync("MaintenanceMode", "true");

        Assert.Equal(first.Id, second.Id);
    }

    [Fact]
    public async Task Execute_EmptyValue_AllowedAndStored()
    {
        var builder = new AdminBuilder();

        var result = await builder.UpsertSystemSetting.ExecuteAsync("BannerMessage", string.Empty);

        Assert.Equal(string.Empty, result.SettingValue);
    }

    #endregion
}
