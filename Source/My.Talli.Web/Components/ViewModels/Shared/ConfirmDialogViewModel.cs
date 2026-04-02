namespace My.Talli.Web.Components.ViewModels.Shared;

using Microsoft.AspNetCore.Components;

/// <summary>View Model</summary>
public class ConfirmDialogViewModel : ComponentBase
{
	#region <Properties>

	[Parameter]
	public RenderFragment? ChildContent { get; set; }

	[Parameter]
	public string ConfirmStyle { get; set; } = "primary";

	[Parameter]
	public EventCallback OnCancel { get; set; }

	[Parameter]
	public EventCallback OnConfirm { get; set; }

	[Parameter]
	public bool Visible { get; set; }

	public string ConfirmCss => ConfirmStyle == "danger" ? "confirm-danger" : "confirm-primary";

	#endregion
}
