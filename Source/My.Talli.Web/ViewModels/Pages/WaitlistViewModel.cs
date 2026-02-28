using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace My.Talli.Web.ViewModels.Pages;

public class WaitlistViewModel : ComponentBase
{
    #region <Variables>

    [CascadingParameter]
    private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    #endregion

    #region <Properties>

    public string UserName { get; private set; } = string.Empty;

    #endregion

    #region <Events>

    protected override async Task OnInitializedAsync()
    {
        if (AuthenticationStateTask is not null)
        {
            var authState = await AuthenticationStateTask;
            var user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                UserName = user.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            }
        }
    }

    #endregion
}
