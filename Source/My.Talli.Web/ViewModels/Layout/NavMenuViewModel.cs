namespace My.Talli.Web.ViewModels.Layout;

using Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

/// <summary>View Model</summary>
public class NavMenuViewModel : ComponentBase
{
	#region <Variables>

	[CascadingParameter]
	private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

	#endregion

	#region <Properties>

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

		var info = UserClaimsHelper.Resolve(principal);

		UserEmail = info.Email;
		UserFullName = info.FullName;
		UserInitials = info.Initials;
	}


	#endregion
}
