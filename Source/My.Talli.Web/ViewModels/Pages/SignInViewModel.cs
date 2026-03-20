namespace My.Talli.Web.ViewModels.Pages;

using Microsoft.AspNetCore.Components;

/// <summary>View Model</summary>
public class SignInViewModel : ComponentBase
{
    #region <Variables>

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    #endregion

    #region <Properties>

    protected bool IsSigningIn { get; private set; }

    protected string? SigningInProvider { get; private set; }

    #endregion

    #region <Methods>

    protected void HandleAppleSignIn()
    {
        if (IsSigningIn) return;

        IsSigningIn = true;
        SigningInProvider = "Apple";
        Navigation.NavigateTo("/api/auth/login/apple", forceLoad: true);
    }

    protected void HandleGoogleSignIn()
    {
        if (IsSigningIn) return;

        IsSigningIn = true;
        SigningInProvider = "Google";
        Navigation.NavigateTo("/api/auth/login/google", forceLoad: true);
    }

    protected void HandleMicrosoftSignIn()
    {
        if (IsSigningIn) return;

        IsSigningIn = true;
        SigningInProvider = "Microsoft";
        Navigation.NavigateTo("/api/auth/login/microsoft", forceLoad: true);
    }

    #endregion
}
