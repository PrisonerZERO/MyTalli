namespace My.Talli.Web.ViewModels.Pages;

using Domain.Framework;
using Domain.Repositories;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>View Model</summary>
public class SubscriptionViewModel : ComponentBase
{
    #region <Variables>

    [CascadingParameter]
    private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private RepositoryAdapterAsync<MODELS.Product, ENTITIES.Product> ProductAdapter { get; set; } = default!;

    [Inject]
    private RepositoryAdapterAsync<MODELS.Subscription, ENTITIES.Subscription> SubscriptionAdapter { get; set; } = default!;


    #endregion

    #region <Properties>

    public string CancelsOnDate { get; private set; } = string.Empty;

    public bool IsCancelling { get; private set; }

    public bool IsFreeUser { get; private set; }

    public bool IsLoading { get; private set; } = true;

    public string MemberSince { get; private set; } = string.Empty;

    public string NextBillingDate { get; private set; } = string.Empty;

    public string PlanName { get; private set; } = string.Empty;

    public string PlanPeriod { get; private set; } = string.Empty;

    public string PlanPrice { get; private set; } = string.Empty;

    public string PlanStatus { get; private set; } = string.Empty;


    #endregion

    #region <Events>

    protected override async Task OnInitializedAsync()
    {
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

        var subscription = (await SubscriptionAdapter.FindAsync(
            x => x.UserId == userId
                && (x.Status == SubscriptionStatuses.Active || x.Status == SubscriptionStatuses.Cancelling)))
            .FirstOrDefault();

        if (subscription is null)
        {
            IsFreeUser = true;
            PlanName = "Free";
            PlanStatus = "Active";
            IsLoading = false;
            return;
        }

        var product = await ProductAdapter.GetByIdAsync(subscription.ProductId);

        IsCancelling = subscription.Status == SubscriptionStatuses.Cancelling;
        CancelsOnDate = subscription.EndDate.ToString("MMM d, yyyy");
        PlanName = "Pro";
        PlanPrice = product is not null ? $"${product.VendorPrice:F0}" : string.Empty;
        PlanPeriod = product?.ProductName.Contains("Monthly") == true ? "/mo" : "/yr";
        PlanStatus = IsCancelling ? "Cancelling" : subscription.Status;
        NextBillingDate = subscription.RenewalDate.ToString("MMM d, yyyy");
        MemberSince = subscription.StartDate.ToString("MMM d, yyyy");
        IsLoading = false;
    }


    #endregion

    #region <Methods>

    protected void HandleCancel()
    {
        Navigation.NavigateTo("/subscription/cancel");
    }

    protected void HandleChangePlan()
    {
        Navigation.NavigateTo("/upgrade");
    }

    protected void HandleManageBilling()
    {
        Navigation.NavigateTo("/api/billing/create-portal-session", forceLoad: true);
    }

    protected void HandleUpgrade()
    {
        Navigation.NavigateTo("/upgrade");
    }


    #endregion
}
