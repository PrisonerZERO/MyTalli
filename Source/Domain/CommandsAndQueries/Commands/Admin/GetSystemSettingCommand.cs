namespace My.Talli.Domain.Commands.Admin;

using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class GetSystemSettingCommand
{
    #region <Variables>

    private readonly RepositoryAdapterAsync<SystemSetting, ENTITIES.SystemSetting> _systemSettingAdapter;

    #endregion

    #region <Constructors>

    public GetSystemSettingCommand(RepositoryAdapterAsync<SystemSetting, ENTITIES.SystemSetting> systemSettingAdapter)
    {
        _systemSettingAdapter = systemSettingAdapter;
    }

    #endregion

    #region <Methods>

    public async Task<string?> ExecuteAsync(string settingKey)
    {
        var match = (await _systemSettingAdapter.FindAsync(s => s.SettingKey == settingKey)).FirstOrDefault();
        return match?.SettingValue;
    }

    #endregion
}
