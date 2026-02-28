using Microsoft.AspNetCore.Components;

namespace My.Talli.Web.ViewModels.Pages;

public class SignInViewModel : ComponentBase
{
    protected bool ShowComingSoon { get; set; }

    protected void HandleGoogleSignIn()
    {
        ShowComingSoon = true;
        // TODO: Wire up Google OAuth authentication
    }

    protected void HandleAppleSignIn()
    {
        ShowComingSoon = true;
        // TODO: Wire up Apple Sign-In authentication
    }
}
