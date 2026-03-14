namespace My.Talli.Web.ViewModels.Pages;

using Microsoft.AspNetCore.Components;

/// <summary>View Model</summary>
public class SubscriptionViewModel : ComponentBase
{
    #region <Variables>

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    #endregion

    #region <Properties>

    public string MemberSince { get; private set; } = string.Empty;

    public string NextBillingDate { get; private set; } = string.Empty;

    public string PlanName { get; private set; } = string.Empty;

    public string PlanPeriod { get; private set; } = string.Empty;

    public string PlanPrice { get; private set; } = string.Empty;

    public string PlanStatus { get; private set; } = string.Empty;

    #endregion

    #region <Events>

    protected override void OnInitialized()
    {
        // Mock data — replace with real subscription lookup when database exists
        PlanName = "Pro";
        PlanPrice = "$99";
        PlanPeriod = "/yr";
        PlanStatus = "Active";
        NextBillingDate = "Apr 7, 2026";
        MemberSince = "Jan 15, 2026";
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

    #endregion
}
