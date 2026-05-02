namespace My.Talli.Web.ViewModels.Pages;

using Microsoft.AspNetCore.Components;
using Web.Services.Admin;

/// <summary>View Model</summary>
public class MaintenanceViewModel : ComponentBase
{
	#region <Variables>

	[Inject]
	private IMaintenanceModeService MaintenanceModeService { get; set; } = default!;


	#endregion

	#region <Properties>

	public bool ShowReturnLink => !MaintenanceModeService.IsEnabled;


	#endregion
}
