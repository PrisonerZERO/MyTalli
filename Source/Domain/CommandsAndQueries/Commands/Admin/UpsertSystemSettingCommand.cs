namespace My.Talli.Domain.Commands.Admin;

using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Command</summary>
public class UpsertSystemSettingCommand
{
    #region <Variables>

    private readonly RepositoryAdapterAsync<SystemSetting, ENTITIES.SystemSetting> _systemSettingAdapter;

    #endregion

    #region <Constructors>

    public UpsertSystemSettingCommand(RepositoryAdapterAsync<SystemSetting, ENTITIES.SystemSetting> systemSettingAdapter)
    {
        _systemSettingAdapter = systemSettingAdapter;
    }

    #endregion

    #region <Methods>

    public async Task<SystemSetting> ExecuteAsync(string settingKey, string settingValue)
    {
        var existing = (await _systemSettingAdapter.FindAsync(s => s.SettingKey == settingKey)).FirstOrDefault();

        if (existing is not null)
        {
            existing.SettingValue = settingValue;
            return await _systemSettingAdapter.UpdateAsync(existing);
        }

        return await _systemSettingAdapter.InsertAsync(new SystemSetting
        {
            SettingKey = settingKey,
            SettingValue = settingValue
        });
    }

    #endregion
}
