namespace My.Talli.Web.ViewModels.Shared;

using Microsoft.AspNetCore.Components;

/// <summary>View Model</summary>
public class PagerViewModel : ComponentBase
{
	#region <Parameters>

	[Parameter]
	public int CurrentPage { get; set; } = 1;

	[Parameter]
	public EventCallback<int> OnGoToPage { get; set; }

	[Parameter]
	public int TotalPages { get; set; } = 1;

	#endregion

	#region <Methods>

	protected IEnumerable<PagerSlot> GetSlots()
	{
		if (TotalPages <= 7)
		{
			for (var i = 1; i <= TotalPages; i++)
				yield return PagerSlot.Page(i);

			yield break;
		}

		// Windowed: always show first + last; show current ± 1; insert ellipses around the window.
		var windowStart = Math.Max(2, CurrentPage - 1);
		var windowEnd = Math.Min(TotalPages - 1, CurrentPage + 1);

		yield return PagerSlot.Page(1);

		if (windowStart > 2)
			yield return PagerSlot.Ellipsis();

		for (var i = windowStart; i <= windowEnd; i++)
			yield return PagerSlot.Page(i);

		if (windowEnd < TotalPages - 1)
			yield return PagerSlot.Ellipsis();

		yield return PagerSlot.Page(TotalPages);
	}

	protected async Task GoTo(int page)
	{
		if (page < 1 || page > TotalPages || page == CurrentPage) return;
		await OnGoToPage.InvokeAsync(page);
	}

	protected async Task GoPrevious() => await GoTo(CurrentPage - 1);

	protected async Task GoNext() => await GoTo(CurrentPage + 1);

	#endregion

	#region <Nested>

	protected record PagerSlot(int? PageNumber, bool IsEllipsis)
	{
		public static PagerSlot Page(int n) => new(n, false);
		public static PagerSlot Ellipsis() => new(null, true);
	}

	#endregion
}
