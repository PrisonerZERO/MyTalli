namespace My.Talli.Web.ViewModels.Shared;

using Microsoft.AspNetCore.Components;

/// <summary>View Model</summary>
public class ExportCardViewModel : ComponentBase
{
	#region <Properties>

	[Parameter]
	public string Description { get; set; } = string.Empty;

	[Parameter]
	public string DownloadUrl { get; set; } = string.Empty;

	[Parameter]
	public string Label { get; set; } = string.Empty;

	[Parameter]
	public int RowCount { get; set; }


	#endregion
}
