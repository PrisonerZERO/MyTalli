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
        // TODO: Wire up Apple Sign-In authentication
        Navigation.NavigateTo("/waitlist");
    }

    protected void HandleGoogleSignIn()
    {
        Navigation.NavigateTo("/api/auth/login/google", forceLoad: true);
    }

    protected void HandleMicrosoftSignIn()
    {
        // TODO: Wire up Microsoft OAuth authentication
        Navigation.NavigateTo("/waitlist");
    }

    #endregion
}
