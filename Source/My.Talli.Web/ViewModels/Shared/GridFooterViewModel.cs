namespace My.Talli.Web.ViewModels.Shared;

using Microsoft.AspNetCore.Components;

/// <summary>View Model</summary>
public class GridFooterViewModel : ComponentBase
{
	#region <Parameters>

	[Parameter]
	public int CurrentPage { get; set; } = 1;

	[Parameter]
	public string Density { get; set; } = "compact";

	[Parameter]
	public EventCallback<string> OnDensityChanged { get; set; }

	[Parameter]
	public EventCallback<int> OnGoToPage { get; set; }

	[Parameter]
	public EventCallback<int> OnPageSizeChanged { get; set; }

	[Parameter]
	public string PageSelectId { get; set; } = "page-select";

	[Parameter]
	public int PageSize { get; set; } = 50;

	[Parameter]
	public int[] PageSizeOptions { get; set; } = [10, 25, 50];

	[Parameter]
	public int TotalCount { get; set; }

	[Parameter]
	public int TotalPages { get; set; } = 1;

	#endregion

	#region <Properties>

	protected int RangeStart => TotalCount == 0 ? 0 : (CurrentPage - 1) * PageSize + 1;

	protected int RangeEnd => Math.Min(CurrentPage * PageSize, TotalCount);

	#endregion

	#region <Methods>

	protected async Task HandlePageSizeChange(ChangeEventArgs e)
	{
		if (int.TryParse(e.Value?.ToString(), out var size))
			await OnPageSizeChanged.InvokeAsync(size);
	}

	protected async Task HandleDensityClick(string density) => await OnDensityChanged.InvokeAsync(density);

	#endregion
}
