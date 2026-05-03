namespace My.Talli.UnitTesting.Infrastructure.Stubs;

using My.Talli.Web.Services.Admin;

/// <summary>Stub</summary>
public class MaintenanceModeServiceStub : IMaintenanceModeService
{
	#region <Properties>

	public bool IsEnabled { get; set; }


	#endregion

	#region <Events>

	public event Action<bool>? StateChanged;


	#endregion

	#region <Methods>

	public Task RefreshFromDbAsync() => Task.CompletedTask;

	public Task SetEnabledAsync(bool value, long actingUserId)
	{
		var changed = IsEnabled != value;
		IsEnabled = value;

		if (changed)
			StateChanged?.Invoke(value);

		return Task.CompletedTask;
	}


	#endregion
}
