namespace My.Talli.Web.ViewModels.Shared;

using Microsoft.AspNetCore.Components;

/// <summary>View Model</summary>
public class BrandHeaderViewModel : ComponentBase
{
    #region <Properties>

    [Parameter]
    public RenderFragment? ChildContent { get; set; }


    #endregion
}
