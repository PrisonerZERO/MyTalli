namespace My.Talli.Web.ViewModels.Shared;

using Domain.Commands.Billing;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

/// <summary>View Model</summary>
public class ExpirationBannerViewModel : ComponentBase
{
	#region <Variables>

	[CascadingParameter]
	private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

	[Inject]
	private AcknowledgeExpirationCommand AcknowledgeExpiration { get; set; } = default!;

	[Inject]
	private FindExpiredUnacknowledgedSubscriptionCommand FindExpired { get; set; } = default!;

	[Inject]
	private NavigationManager NavigationManager { get; set; } = default!;

	private long _subscriptionId;
	private long _userId;

	#endregion

	#region <Properties>

	public string EndDateDisplay { get; private set; } = string.Empty;

	public string PlanName { get; private set; } = string.Empty;

	public bool ShowBanner { get; private set; }

	#endregion

	#region <Events>

	protected override async Task OnInitializedAsync()
	{
		var authState = await AuthenticationStateTask;
		var principal = authState.User;

		if (principal.Identity?.IsAuthenticated != true)
			return;

		var userIdClaim = principal.FindFirst("UserId")?.Value;
		if (userIdClaim is null || !long.TryParse(userIdClaim, out var userId))
			return;

		_userId = userId;

		var expired = await FindExpired.ExecuteAsync(userId);
		if (expired is null)
			return;

		_subscriptionId = expired.Id;
		PlanName = expired.ProductId == 2 ? "Pro Yearly" : "Pro Monthly";
		EndDateDisplay = expired.EndDate.ToString("MMM d, yyyy");
		ShowBanner = true;
	}

	#endregion

	#region <Methods>

	public async Task DismissAsync()
	{
		if (!ShowBanner) return;

		await AcknowledgeExpiration.ExecuteAsync(_subscriptionId, _userId);
		ShowBanner = false;
	}

	public void Renew()
	{
		NavigationManager.NavigateTo("/my-plan");
	}

	#endregion
}
