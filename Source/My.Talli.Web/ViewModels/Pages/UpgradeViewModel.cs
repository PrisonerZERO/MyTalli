using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;

namespace My.Talli.Web.ViewModels.Pages;

/// <summary>View Model</summary>
public class UpgradeViewModel : ComponentBase
{
    #region <Variables>

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    #endregion

    #region <Properties>

    public string SelectedPlan { get; private set; } = "yearly";

    public bool ShowCancelledToast { get; private set; }

    public bool ShowSuccessToast { get; private set; }

    #endregion

    #region <Events>

    protected override void OnInitialized()
    {
        var uri = new Uri(Navigation.Uri);
        var query = QueryHelpers.ParseQuery(uri.Query);

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
            }
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && (ShowSuccessToast || ShowCancelledToast))
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
