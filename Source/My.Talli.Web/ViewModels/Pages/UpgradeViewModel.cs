namespace My.Talli.Web.ViewModels.Pages;

using Domain.Framework;
using Domain.Repositories;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>View Model</summary>
public class UpgradeViewModel : ComponentBase
{
	#region <Variables>

	[CascadingParameter]
	private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

	[Inject]
	private IJSRuntime JS { get; set; } = default!;

	[Inject]
	private NavigationManager Navigation { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.Product, ENTITIES.Product> ProductAdapter { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.Subscription, ENTITIES.Subscription> SubscriptionAdapter { get; set; } = default!;


	#endregion

	#region <Properties>

	public string CurrentPlanPeriod { get; private set; } = string.Empty;

	public bool IsCurrentPeriodSelected => IsProSubscriber && SelectedPlan == CurrentPlanPeriod;

	public bool IsProSubscriber { get; private set; }

	public string SelectedPlan { get; private set; } = "yearly";

	public bool ShowCancelledToast { get; private set; }

	public bool ShowSuccessToast { get; private set; }

	public bool ShowSwitchedToast { get; private set; }

	public string SwitchButtonText => SelectedPlan == "monthly" ? "Switch to Monthly" : "Switch to Yearly";


	#endregion

	#region <Events>

	protected override async Task OnInitializedAsync()
	{
		var query = QueryHelpers.ParseQuery(new Uri(Navigation.Uri).Query);

		if (query.TryGetValue("status", out var status))
		{
			switch (status.ToString())
			{
				case "success":
					ShowSuccessToast = true;
					break;
				case "cancelled":
					ShowCancelledToast = true;
					break;
				case "switched":
					ShowSwitchedToast = true;
					break;
			}
		}

		var authState = await AuthenticationStateTask;
		var principal = authState.User;

		if (principal.Identity?.IsAuthenticated != true)
			return;

		var userIdClaim = principal.FindFirst("UserId")?.Value;
		if (userIdClaim is null || !long.TryParse(userIdClaim, out var userId))
			return;

		var subscription = (await SubscriptionAdapter.FindAsync(x => x.UserId == userId && (x.Status == SubscriptionStatuses.Active || x.Status == SubscriptionStatuses.Cancelling))).FirstOrDefault();
		if (subscription is null)
			return;

		IsProSubscriber = true;

		var product = await ProductAdapter.GetByIdAsync(subscription.ProductId);

		if (product?.ProductName.Contains("Monthly") == true)
		{
			CurrentPlanPeriod = "monthly";
			SelectedPlan = "monthly";
		}
		else
		{
			CurrentPlanPeriod = "yearly";
			SelectedPlan = "yearly";
		}
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender && (ShowSuccessToast || ShowCancelledToast || ShowSwitchedToast))
		{
			await JS.InvokeVoidAsync("history.replaceState", null, "", "/upgrade");
		}
	}


	#endregion

	#region <Methods>

	protected void DismissToast()
	{
		ShowCancelledToast = false;
		ShowSuccessToast = false;
		ShowSwitchedToast = false;
	}

	protected void HandleManagePlan()
	{
		Navigation.NavigateTo("/api/billing/create-portal-session", forceLoad: true);
	}

	protected void HandleSwitchPlan()
	{
		Navigation.NavigateTo(
			$"/api/billing/switch-plan?plan={SelectedPlan}",
			forceLoad: true);
	}

	protected void HandleUpgrade()
	{
		Navigation.NavigateTo(
			$"/api/billing/create-checkout-session?plan={SelectedPlan}",
			forceLoad: true);
	}

	protected void SelectPlan(string plan)
	{
		SelectedPlan = plan;
	}


	#endregion
}
