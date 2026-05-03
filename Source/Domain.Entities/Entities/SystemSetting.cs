namespace My.Talli.Domain.Entities;

/// <summary>Entity</summary>
public class SystemSetting : DefaultEntity
{
	#region <Properties>

	public string SettingKey { get; set; } = string.Empty;

	public string SettingValue { get; set; } = string.Empty;


	#endregion
}
