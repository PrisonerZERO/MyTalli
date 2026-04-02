namespace My.Talli.Web.Components.ViewModels.Layout;

using Domain.Components.JsonSerializers;
using Domain.Framework;
using Domain.Repositories;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Services.Identity;
using System.Security.Claims;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>View Model</summary>
public class NavMenuViewModel : ComponentBase
{
	#region <Variables>

	[CascadingParameter]
	private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

	[Inject]
	private IJSRuntime JsRuntime { get; set; } = default!;

	[Inject]
	private UserPreferencesJsonSerializer PreferencesSerializer { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.Subscription, ENTITIES.Subscription> SubscriptionAdapter { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.User, ENTITIES.User> UserAdapter { get; set; } = default!;

	[Inject]
	private UserDisplayCache UserDisplayCache { get; set; } = default!;

	private bool _themeApplied;

	private long? _userId;

	#endregion

	#region <Properties>

	public bool IsAdmin { get; private set; }

	public bool IsProSubscriber { get; private set; }

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

		var proSub = (await SubscriptionAdapter.FindAsync(s =>
			s.UserId == userId
			&& (s.ProductId == 1 || s.ProductId == 2)
			&& (s.Status == SubscriptionStatuses.Active || s.Status == SubscriptionStatuses.Cancelling)))
			.FirstOrDefault();

		IsProSubscriber = proSub is not null;

		_userId = userId;
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!_themeApplied && _userId.HasValue)
		{
			_themeApplied = true;
			await ApplyThemeAsync(_userId.Value);
		}
	}


	#endregion

	#region <Methods>

	private async Task ApplyThemeAsync(long userId)
	{
		var user = await UserAdapter.GetByIdAsync(userId);

		if (user is null)
			return;

		var preferences = PreferencesSerializer.Deserialize(user.UserPreferences);
		var mode = preferences.DarkMode ?? "system";

		await JsRuntime.InvokeVoidAsync("themeManager.apply", mode);
	}

	#endregion
}
