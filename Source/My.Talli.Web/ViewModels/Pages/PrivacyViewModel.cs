namespace My.Talli.Web.ViewModels.Pages;

using Microsoft.AspNetCore.Components;

/// <summary>View Model</summary>
public class PrivacyViewModel : ComponentBase
{
    #region <Variables>

    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }


    #endregion

    #region <Properties>

    protected string BackLinkHref => IsAuthenticated ? "/dashboard" : "/";

    protected string BackLinkText => IsAuthenticated ? "Back to Dashboard" : "Back to Homepage";

    protected string EffectiveDate => "April 25, 2026";

    protected string LastUpdated => "April 25, 2026";

    private bool IsAuthenticated => HttpContext?.User.Identity?.IsAuthenticated ?? false;


    #endregion
}
