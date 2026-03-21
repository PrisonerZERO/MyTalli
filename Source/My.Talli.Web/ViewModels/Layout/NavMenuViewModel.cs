namespace My.Talli.Web.ViewModels.Layout;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

/// <summary>View Model</summary>
public class NavMenuViewModel : ComponentBase
{
    #region <Variables>

    [CascadingParameter]
    private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    #endregion

    #region <Properties>

    public string UserEmail { get; private set; } = string.Empty;

    public string UserFullName { get; private set; } = string.Empty;

    public string UserInitials { get; private set; } = string.Empty;


    #endregion

    #region <Events>

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateTask;
        var user = authState.User;

        if (user.Identity?.IsAuthenticated != true)
            return;

        var firstName = user.FindFirst(ClaimTypes.GivenName)?.Value ?? string.Empty;
        var lastName = user.FindFirst(ClaimTypes.Surname)?.Value ?? string.Empty;

        UserFullName = user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
        UserEmail = user.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        UserInitials = $"{(firstName.Length > 0 ? firstName[0] : ' ')}{(lastName.Length > 0 ? lastName[0] : ' ')}".Trim();
    }


    #endregion
}
