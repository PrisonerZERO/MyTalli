namespace My.Talli.Web.ViewModels.Shared;

using Microsoft.AspNetCore.Components;

/// <summary>View Model</summary>
public class GridStatBarViewModel : ComponentBase
{
	#region <Parameters>

	[Parameter]
	public int Count { get; set; }

	[Parameter]
	public string PeriodLabel { get; set; } = "current period";

	[Parameter]
	public string PluralLabel { get; set; } = "items";

	[Parameter]
	public string SingularLabel { get; set; } = "item";

	[Parameter]
	public decimal Total { get; set; }

	[Parameter]
	public string TotalFormat { get; set; } = "C0";

	[Parameter]
	public string TotalSuffix { get; set; } = string.Empty;

	#endregion
}
