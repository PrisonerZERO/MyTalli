namespace My.Talli.Web.Services.Admin;

/// <summary>Service</summary>
public interface IMaintenanceModeService
{
	#region <Properties>

	bool IsEnabled { get; }


	#endregion

	#region <Events>

	event Action<bool> StateChanged;


	#endregion

	#region <Methods>

	Task RefreshFromDbAsync();

	Task SetEnabledAsync(bool value, long actingUserId);


	#endregion
}
