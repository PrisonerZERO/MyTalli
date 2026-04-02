namespace My.Talli.Web.Components.ViewModels.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

/// <summary>View Model</summary>
public class CancelSubscriptionViewModel : ComponentBase
{
    #region <Variables>

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Inject]
    private ILogger<CancelSubscriptionViewModel> Logger { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;


    #endregion

    #region <Properties>

    public string SelectedReason { get; set; } = string.Empty;

    public bool ShowKeptToast { get; private set; }


    #endregion

    #region <Events>

    protected override void OnInitialized()
    {
        var uri = new Uri(Navigation.Uri);
        var query = QueryHelpers.ParseQuery(uri.Query);

        if (query.TryGetValue("status", out var status) && status == "kept")
        {
            ShowKeptToast = true;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && ShowKeptToast)
        {
            await JS.InvokeVoidAsync("history.replaceState", null, "", "/subscription/cancel");
        }
    }


    #endregion

    #region <Methods>

    protected void DismissToast()
    {
        ShowKeptToast = false;
    }

    protected void HandleCancel()
    {
        Logger.LogInformation("User initiated cancellation. Reason: {Reason}",
            string.IsNullOrEmpty(SelectedReason) ? "Not specified" : SelectedReason);

        Navigation.NavigateTo("/api/billing/create-portal-session", forceLoad: true);
    }

    protected void HandleKeep()
    {
        Navigation.NavigateTo("/dashboard");
    }


    #endregion
}
