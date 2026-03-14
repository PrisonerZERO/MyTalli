using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;

namespace My.Talli.Web.ViewModels.Pages;

/// <summary>View Model</summary>
public class LandingPageViewModel : ComponentBase, IAsyncDisposable
{
    #region <Variables>

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private IJSObjectReference? _module;

    #endregion

    #region <Properties>

    public bool ShowSignedOutToast { get; private set; }

    public string SignedOutName { get; private set; } = string.Empty;

    #endregion

    #region <Events>

    protected override void OnInitialized()
    {
        var uri = new Uri(Navigation.Uri);
        var query = QueryHelpers.ParseQuery(uri.Query);

        if (query.ContainsKey("signed-out"))
        {
            ShowSignedOutToast = true;

            if (query.TryGetValue("name", out var nameValue) && !string.IsNullOrEmpty(nameValue))
            {
                SignedOutName = nameValue!;
            }
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (ShowSignedOutToast)
            {
                await JS.InvokeVoidAsync("history.replaceState", null, "", "/");
            }

            _module = await JS.InvokeAsync<IJSObjectReference>("import", "./js/landing.js");
            await _module.InvokeVoidAsync("initLanding");
        }
    }

    #endregion

    #region <Methods>

    protected void DismissToast()
    {
        ShowSignedOutToast = false;
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            try
            {
                await _module.InvokeVoidAsync("dispose");
                await _module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Circuit may already be disconnected
            }
        }
    }

    protected async Task ScrollToSection(string sectionId)
    {
        if (_module is not null)
            await _module.InvokeVoidAsync("scrollToSection", sectionId);
    }

    protected async Task ScrollToTop()
    {
        if (_module is not null)
            await _module.InvokeVoidAsync("scrollToTop");
    }

    #endregion
}
