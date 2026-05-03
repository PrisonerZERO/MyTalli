namespace My.Talli.Web.ViewModels.Pages;

using Microsoft.AspNetCore.Components;
using Web.Services.Admin;

/// <summary>View Model</summary>
public class MaintenanceViewModel : ComponentBase, IDisposable
{
	#region <Variables>

	[Inject]
	private IMaintenanceModeService MaintenanceModeService { get; set; } = default!;

	[Inject]
	private NavigationManager NavigationManager { get; set; } = default!;


	#endregion

	#region <Properties>

	public bool ShowReturnLink => !MaintenanceModeService.IsEnabled;


	#endregion

	#region <Events>

	protected override void OnInitialized()
	{
		MaintenanceModeService.StateChanged += OnStateChanged;
	}

	public void Dispose()
	{
		MaintenanceModeService.StateChanged -= OnStateChanged;

		GC.SuppressFinalize(this);
	}


	#endregion

	#region <Methods>

	private void OnStateChanged(bool newValue)
	{
		if (!newValue)
		{
			InvokeAsync(() => NavigationManager.NavigateTo("/dashboard", forceLoad: true));
			return;
		}

		InvokeAsync(StateHasChanged);
	}


	#endregion
}
