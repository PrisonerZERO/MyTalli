using Microsoft.AspNetCore.Components;

namespace My.Talli.Web.ViewModels.Pages;

public class SignInViewModel : ComponentBase
{
    #region <Variables>

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    #endregion

    #region <Methods>

    protected void HandleAppleSignIn()
    {
        Navigation.NavigateTo("/api/auth/login/apple", forceLoad: true);
    }

    protected void HandleGoogleSignIn()
    {
        Navigation.NavigateTo("/api/auth/login/google", forceLoad: true);
    }

    protected void HandleMicrosoftSignIn()
    {
        Navigation.NavigateTo("/api/auth/login/microsoft", forceLoad: true);
    }

    #endregion
}
