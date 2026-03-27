namespace My.Talli.Web.ViewModels.Layout;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Services.Identity;
using System.Security.Claims;

/// <summary>View Model</summary>
public class NavMenuViewModel : ComponentBase
{
	#region <Variables>

	[CascadingParameter]
	private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

	[Inject]
	private UserDisplayCache UserDisplayCache { get; set; } = default!;

	#endregion

	#region <Properties>

	public bool IsAdmin { get; private set; }

	public string UserEmail { get; private set; } = string.Empty;

	public string UserFullName { get; private set; } = string.Empty;

	public string UserInitials { get; private set; } = string.Empty;


	#endregion

	#region <Events>

	protected override async Task OnInitializedAsync()
	{
		var authState = await AuthenticationStateTask;
		var principal = authState.User;

		if (principal.Identity?.IsAuthenticated != true)
			return;

		var email = principal.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
		var userIdClaim = principal.FindFirst("UserId")?.Value;

		if (userIdClaim is null || !long.TryParse(userIdClaim, out var userId))
			return;

		IsAdmin = principal.IsInRole("Admin");

		var (info, _) = await UserDisplayCache.GetOrLoadAsync(userId, email);

		UserEmail = info.Email;
		UserFullName = info.FullName;
		UserInitials = info.Initials;
	}


	#endregion
}
