using Microsoft.AspNetCore.Components;

namespace My.Talli.Web.ViewModels.Shared;

/// <summary>View Model</summary>
public class BrandHeaderViewModel : ComponentBase
{
    #region <Properties>

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    #endregion
}
