namespace My.Talli.Web.ViewModels.Shared;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Web.Services.Admin;

/// <summary>View Model</summary>
public class MaintenanceBannerViewModel : ComponentBase, IDisposable
{
	#region <Variables>

	[CascadingParameter]
	private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

	[Inject]
	private ICircuitTracker CircuitTracker { get; set; } = default!;

	[Inject]
	private IMaintenanceModeService MaintenanceModeService { get; set; } = default!;

	[Inject]
	private NavigationManager NavigationManager { get; set; } = default!;

	private long _actingUserId;
	private string _sessionId = string.Empty;


	#endregion

	#region <Properties>

	public bool IsAdmin { get; private set; }

	public bool IsConfirmingTurnOff { get; private set; }

	public bool IsRegistered { get; private set; }

	public int InAppNonAdminCount => CircuitTracker.InAppNonAdminCount;

	public bool IsMaintenanceOn => MaintenanceModeService.IsEnabled;

	public bool ShowBanner => IsAdmin && IsMaintenanceOn;


	#endregion

	#region <Events>

	protected override async Task OnInitializedAsync()
	{
		var authState = await AuthenticationStateTask;
		var principal = authState.User;

		IsAdmin = principal.Identity?.IsAuthenticated == true && principal.IsInRole("Admin");
		_actingUserId = ResolveUserId(principal);
		_sessionId = Guid.NewGuid().ToString("N");

		CircuitTracker.RegisterInAppSession(_sessionId, IsAdmin);
		IsRegistered = true;

		MaintenanceModeService.StateChanged += OnMaintenanceStateChanged;
		CircuitTracker.CountChanged += OnCircuitCountChanged;
	}

	public void Dispose()
	{
		if (IsRegistered)
			CircuitTracker.UnregisterInAppSession(_sessionId);

		MaintenanceModeService.StateChanged -= OnMaintenanceStateChanged;
		CircuitTracker.CountChanged -= OnCircuitCountChanged;

		GC.SuppressFinalize(this);
	}


	#endregion

	#region <Methods>

	public void StartConfirmTurnOff() => IsConfirmingTurnOff = true;

	public void CancelTurnOff() => IsConfirmingTurnOff = false;

	public async Task ConfirmTurnOffAsync()
	{
		IsConfirmingTurnOff = false;
		await MaintenanceModeService.SetEnabledAsync(false, _actingUserId);
	}

	private static long ResolveUserId(ClaimsPrincipal principal)
	{
		var raw = principal.FindFirst("UserId")?.Value
			?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

		return long.TryParse(raw, out var userId) ? userId : 0L;
	}

	private void OnMaintenanceStateChanged(bool newValue)
	{
		// Non-admins get pushed to /maintenance immediately when MM flips on
		if (newValue && !IsAdmin)
		{
			InvokeAsync(() => NavigationManager.NavigateTo("/maintenance", forceLoad: true));
			return;
		}

		InvokeAsync(StateHasChanged);
	}

	private void OnCircuitCountChanged()
	{
		InvokeAsync(StateHasChanged);
	}


	#endregion
}
