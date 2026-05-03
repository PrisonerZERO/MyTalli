namespace My.Talli.Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Mapper</summary>
public class SystemSettingMapper : EntityMapper<MODELS.SystemSetting, ENTITIES.SystemSetting>
{
	#region <Methods>

	public override void ApplyTo(ENTITIES.SystemSetting source, MODELS.SystemSetting destination)
	{
		destination.Id = source.Id;
		destination.IsDeleted = source.IsDeleted;
		destination.IsVisible = source.IsVisible;
		destination.SettingKey = source.SettingKey;
		destination.SettingValue = source.SettingValue;
	}

	public override ENTITIES.SystemSetting ToEntity(MODELS.SystemSetting model) => new()
	{
		Id = model.Id,
		IsDeleted = model.IsDeleted,
		IsVisible = model.IsVisible,
		SettingKey = model.SettingKey,
		SettingValue = model.SettingValue,
	};

	public override MODELS.SystemSetting ToModel(ENTITIES.SystemSetting entity) => new()
	{
		Id = entity.Id,
		IsDeleted = entity.IsDeleted,
		IsVisible = entity.IsVisible,
		SettingKey = entity.SettingKey,
		SettingValue = entity.SettingValue,
	};


	#endregion
}
