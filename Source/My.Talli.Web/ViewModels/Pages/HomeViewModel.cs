using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace My.Talli.Web.ViewModels.Pages;

public class HomeViewModel : ComponentBase, IAsyncDisposable
{
    #region <Variables>

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    private IJSObjectReference? _module;

    #endregion

    #region <Events>

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _module = await JS.InvokeAsync<IJSObjectReference>("import", "./js/landing.js");
            await _module.InvokeVoidAsync("initLanding");
        }
    }

    #endregion

    #region <Methods>

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

    protected void HandleWaitlistSubmit()
    {
        // TODO: Implement waitlist signup
    }

    protected async Task HighlightWaitlist()
    {
        if (_module is not null)
            await _module.InvokeVoidAsync("highlightWaitlist");
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
