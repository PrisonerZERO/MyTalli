namespace My.Talli.Web.Components.ViewModels.Shared;

using Microsoft.AspNetCore.Components;

/// <summary>View Model</summary>
public class BrandHeaderViewModel : ComponentBase
{
    #region <Properties>

    [Parameter]
    public RenderFragment? ChildContent { get; set; }


    #endregion
}
