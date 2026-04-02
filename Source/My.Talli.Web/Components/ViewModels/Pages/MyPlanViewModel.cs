namespace My.Talli.Web.Components.ViewModels.Pages;

using Domain.Framework;
using Domain.Repositories;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>View Model</summary>
public class MyPlanViewModel : ComponentBase
{
	#region <Variables>

	private long? _userId;

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

	public List<ModuleItem> ActiveModules { get; private set; } = [];

	public List<ModuleItem> AvailableModules { get; private set; } = [];

	public string CancelsOnDate { get; private set; } = string.Empty;

	public string CurrentPlanPeriod { get; private set; } = string.Empty;

	public bool IsCancelling { get; private set; }

	public bool IsCurrentPeriodSelected => IsProSubscriber && SelectedPlan == CurrentPlanPeriod;

	public bool IsFreeUser { get; private set; }

	public bool IsLoading { get; private set; } = true;

	public bool IsProSubscriber { get; private set; }

	public string MemberSince { get; private set; } = string.Empty;

	public string NextBillingDate { get; private set; } = string.Empty;

	public string PlanName { get; private set; } = string.Empty;

	public string PlanPeriod { get; private set; } = string.Empty;

	public string PlanPrice { get; private set; } = string.Empty;

	public string PlanStatus { get; private set; } = string.Empty;

	public string SelectedPlan { get; private set; } = "yearly";

	public bool ShowCancelledToast { get; private set; }

	public bool ShowSuccessToast { get; private set; }

	public bool ShowSwitchedToast { get; private set; }

	public string SwitchButtonText => SelectedPlan == "monthly" ? "Switch to Monthly" : "Switch to Yearly";

	#endregion

	#region <Events>

	protected override async Task OnInitializedAsync()
	{
		// Toast states from query string
		var query = QueryHelpers.ParseQuery(new Uri(Navigation.Uri).Query);

		if (query.TryGetValue("status", out var status))
		{
			switch (status.ToString())
			{
				case "success": ShowSuccessToast = true; break;
				case "cancelled": ShowCancelledToast = true; break;
				case "switched": ShowSwitchedToast = true; break;
			}
		}

		var authState = await AuthenticationStateTask;
		var principal = authState.User;

		if (principal.Identity?.IsAuthenticated != true)
		{
			IsFreeUser = true;
			PlanName = "Free";
			PlanStatus = "Active";
			IsLoading = false;
			return;
		}

		var userIdClaim = principal.FindFirst("UserId")?.Value;

		if (userIdClaim is null || !long.TryParse(userIdClaim, out var userId))
		{
			IsFreeUser = true;
			PlanName = "Free";
			PlanStatus = "Active";
			IsLoading = false;
			return;
		}

		_userId = userId;

		// Load all active subscriptions for this user
		var subscriptions = (await SubscriptionAdapter.FindAsync(
			x => x.UserId == userId
				&& (x.Status == SubscriptionStatuses.Active || x.Status == SubscriptionStatuses.Cancelling)))
			.ToList();

		// Load all products
		var allProducts = (await ProductAdapter.GetAllAsync()).ToList();
		var productLookup = allProducts.ToDictionary(p => p.Id);

		// Service plan (ProductId 1 = Pro Monthly, ProductId 2 = Pro Yearly)
		var proSubscription = subscriptions.FirstOrDefault(s => s.ProductId == 1 || s.ProductId == 2);

		if (proSubscription is null)
		{
			IsFreeUser = true;
			PlanName = "Free";
			PlanStatus = "Active";
		}
		else
		{
			IsProSubscriber = true;
			IsCancelling = proSubscription.Status == SubscriptionStatuses.Cancelling;
			CancelsOnDate = proSubscription.EndDate.ToString("MMM d, yyyy");
			PlanName = "Pro";
			PlanStatus = IsCancelling ? "Cancelling" : proSubscription.Status;
			NextBillingDate = proSubscription.RenewalDate.ToString("MMM d, yyyy");
			MemberSince = proSubscription.StartDate.ToString("MMM d, yyyy");

			var product = productLookup.GetValueOrDefault(proSubscription.ProductId);
			PlanPrice = product is not null ? $"${product.VendorPrice:F0}" : string.Empty;
			PlanPeriod = product?.ProductName.Contains("Monthly") == true ? "/mo" : "/yr";
			CurrentPlanPeriod = product?.ProductName.Contains("Monthly") == true ? "monthly" : "yearly";
			SelectedPlan = CurrentPlanPeriod;
		}

		// Module subscriptions (ProductId 3+ with ProductType "Software Module")
		var moduleSubscriptions = subscriptions.Where(s => s.ProductId >= 3).ToList();
		var ownedProductIds = moduleSubscriptions.Select(s => s.ProductId).ToHashSet();

		foreach (var sub in moduleSubscriptions)
		{
			var product = productLookup.GetValueOrDefault(sub.ProductId);
			if (product is null) continue;

			ActiveModules.Add(ToModuleItem(product, sub));
		}

		// Available modules (Software Module products not owned)
		var moduleProducts = allProducts.Where(p => p.ProductTypeId == 2 && !ownedProductIds.Contains(p.Id));

		foreach (var product in moduleProducts)
		{
			AvailableModules.Add(new ModuleItem
			{
				ProductId = product.Id,
				Name = product.ProductName,
				Price = $"${product.VendorPrice:F0}",
			});
		}

		IsLoading = false;
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender && (ShowSuccessToast || ShowCancelledToast || ShowSwitchedToast))
		{
			await JS.InvokeVoidAsync("history.replaceState", null, "", "/my-plan");
		}
	}

	#endregion

	#region <Methods>

	public void DismissToast()
	{
		ShowCancelledToast = false;
		ShowSuccessToast = false;
		ShowSwitchedToast = false;
	}

	public void HandleAddModule(long productId)
	{
		Navigation.NavigateTo($"/api/billing/create-checkout-session?product={productId}", forceLoad: true);
	}

	public void HandleCancel()
	{
		Navigation.NavigateTo("/subscription/cancel");
	}

	public void HandleCancelModule(long productId)
	{
		Navigation.NavigateTo("/api/billing/create-portal-session", forceLoad: true);
	}

	public void HandleChangePlan()
	{
		// Scroll handled by anchor or just toggle period
	}

	public void HandleManageBilling()
	{
		Navigation.NavigateTo("/api/billing/create-portal-session", forceLoad: true);
	}

	public void HandleSwitchPlan()
	{
		Navigation.NavigateTo($"/api/billing/switch-plan?plan={SelectedPlan}", forceLoad: true);
	}

	public void HandleUpgrade()
	{
		Navigation.NavigateTo($"/api/billing/create-checkout-session?plan={SelectedPlan}", forceLoad: true);
	}

	public void SelectPlan(string plan)
	{
		SelectedPlan = plan;
	}

	private ModuleItem ToModuleItem(MODELS.Product product, MODELS.Subscription sub)
	{
		return new ModuleItem
		{
			ActiveSince = sub.StartDate.ToString("MMM d, yyyy"),
			IsCancelling = sub.Status == SubscriptionStatuses.Cancelling,
			Name = product.ProductName,
			NextBillingDate = sub.RenewalDate.ToString("MMM d, yyyy"),
			Price = $"${product.VendorPrice:F0}",
			ProductId = product.Id,
			Status = sub.Status == SubscriptionStatuses.Cancelling ? "Cancelling" : "Active",
		};
	}

	#endregion
}

/// <summary>Model</summary>
public class ModuleItem
{
	public string ActiveSince { get; set; } = string.Empty;
	public bool IsCancelling { get; set; }
	public string Name { get; set; } = string.Empty;
	public string NextBillingDate { get; set; } = string.Empty;
	public string Price { get; set; } = string.Empty;
	public long ProductId { get; set; }
	public string Status { get; set; } = string.Empty;
}
