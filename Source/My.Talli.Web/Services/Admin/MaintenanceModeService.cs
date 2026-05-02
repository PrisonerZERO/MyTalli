namespace My.Talli.Web.Services.Admin;

using Domain.Commands.Admin;
using Domain.Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;

/// <summary>Service</summary>
public class MaintenanceModeService : IMaintenanceModeService
{
	#region <Constants>

	public const string MaintenanceModeKey = "MaintenanceMode";


	#endregion

	#region <Variables>

	private readonly IServiceScopeFactory _scopeFactory;
	private readonly Lock _stateLock = new();
	private bool _isEnabled;


	#endregion

	#region <Constructors>

	public MaintenanceModeService(IServiceScopeFactory scopeFactory)
	{
		_scopeFactory = scopeFactory;
	}


	#endregion

	#region <Properties>

	public bool IsEnabled
	{
		get
		{
			lock (_stateLock)
				return _isEnabled;
		}
	}


	#endregion

	#region <Events>

	public event Action<bool>? StateChanged;


	#endregion

	#region <Methods>

	public async Task RefreshFromDbAsync()
	{
		using var scope = _scopeFactory.CreateScope();
		var getCommand = scope.ServiceProvider.GetRequiredService<GetSystemSettingCommand>();

		var raw = await getCommand.ExecuteAsync(MaintenanceModeKey);
		var newValue = string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase);

		UpdateCacheAndRaiseIfChanged(newValue);
	}

	public async Task SetEnabledAsync(bool value, long actingUserId)
	{
		using var scope = _scopeFactory.CreateScope();
		var upsertCommand = scope.ServiceProvider.GetRequiredService<UpsertSystemSettingCommand>();
		var currentUser = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();

		currentUser.Set(actingUserId, string.Empty);
		try
		{
			await upsertCommand.ExecuteAsync(MaintenanceModeKey, value ? "true" : "false");
		}
		finally
		{
			currentUser.Clear();
		}

		UpdateCacheAndRaiseIfChanged(value);
	}

	private void UpdateCacheAndRaiseIfChanged(bool newValue)
	{
		bool changed;

		lock (_stateLock)
		{
			changed = _isEnabled != newValue;
			_isEnabled = newValue;
		}

		if (changed)
			StateChanged?.Invoke(newValue);
	}


	#endregion
}
