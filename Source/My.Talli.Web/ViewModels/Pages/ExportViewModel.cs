namespace My.Talli.Web.ViewModels.Pages;

using Domain.Commands.Billing;
using Domain.Commands.Export;
using Domain.Data.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Web.Commands.Export;

/// <summary>View Model</summary>
public class ExportViewModel : ComponentBase
{
	#region <Variables>

	[CascadingParameter]
	private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

	[Inject]
	private ICurrentUserService CurrentUserService { get; set; } = default!;

	[Inject]
	private IsProSubscriberCommand IsProSubscriberQuery { get; set; } = default!;

	[Inject]
	private GetExportPreviewCommand PreviewCommand { get; set; } = default!;

	[Inject]
	private NavigationManager Navigation { get; set; } = default!;


	#endregion

	#region <Properties>

	public DateTime FromDateLocal { get; set; } = DateTime.Today.AddDays(-90);

	/// <summary>HTML date input <c>min</c> attribute on From — empty string for Pro (no lower floor), or the 30-day-ago date for free users so the OS date picker won't even offer earlier values.</summary>
	public string FromInputMin => IsPro ? string.Empty : DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd");

	/// <summary>HTML date input <c>max</c> attribute — today for everyone (future dates have no meaning for historical-data exports).</summary>
	public string DateInputMax => DateTime.Today.ToString("yyyy-MM-dd");

	/// <summary>HTML date input <c>min</c> attribute on To — empty for Pro, 30-day-ago for free users (so the To picker shows the same window as From).</summary>
	public string ToInputMin => IsPro ? string.Empty : DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd");

	public bool IsLoading { get; private set; } = true;

	public bool IsPro { get; private set; }

	public bool IsRefreshingPreview { get; private set; }

	public ExportPreview? Preview { get; private set; }

	public string SelectedRange { get; private set; } = "90d";

	public DateTime ToDateLocal { get; set; } = DateTime.Today;


	#endregion

	#region <Events>

	protected override async Task OnInitializedAsync()
	{
		var authState = await AuthenticationStateTask;
		var principal = authState.User;

		if (principal.Identity?.IsAuthenticated != true)
		{
			IsLoading = false;
			return;
		}

		var userIdClaim = principal.FindFirst("UserId")?.Value;
		if (!long.TryParse(userIdClaim, out var userId))
		{
			IsLoading = false;
			return;
		}

		// CurrentUserService is scoped per circuit; populate so any audit-touching code works
		CurrentUserService.Set(userId, string.Empty);

		// CSV export is a Pro-only feature — UI replaces download buttons with an Upgrade CTA when not Pro
		IsPro = await IsProSubscriberQuery.ExecuteAsync(userId);

		// Free tier = 30-day history cap. Clamp default range + selection to match the rest of the app.
		if (!IsPro)
		{
			SelectedRange = "30d";
			FromDateLocal = DateTime.Today.AddDays(-30);
			ToDateLocal = DateTime.Today;
		}

		await RefreshPreviewAsync(userId);
		IsLoading = false;
	}


	#endregion

	#region <Methods>

	public async Task SelectRangeAsync(string range)
	{
		// Free tier is capped at the 30-day preset. Defense-in-depth: if a request somehow arrives for a longer range (UI tampering), snap it back.
		if (!IsPro && range != "30d")
			range = "30d";

		SelectedRange = range;

		var today = DateTime.Today;
		switch (range)
		{
			case "30d": FromDateLocal = today.AddDays(-30); ToDateLocal = today; break;
			case "90d": FromDateLocal = today.AddDays(-90); ToDateLocal = today; break;
			case "365d": FromDateLocal = today.AddDays(-365); ToDateLocal = today; break;
			case "ytd": FromDateLocal = new DateTime(today.Year, 1, 1); ToDateLocal = today; break;
			case "all": FromDateLocal = new DateTime(2020, 1, 1); ToDateLocal = today; break;
		}

		await RefreshPreviewIfAuthenticatedAsync();
	}

	public async Task SetCustomRangeAsync()
	{
		var today = DateTime.Today;

		// Universal: To can never be in the future.
		if (ToDateLocal > today) ToDateLocal = today;
		if (FromDateLocal > today) FromDateLocal = today;

		// Free tier: clamp the custom From AND To to the 30-day window.
		if (!IsPro)
		{
			var floor = today.AddDays(-30);
			if (FromDateLocal < floor) FromDateLocal = floor;
			if (ToDateLocal < floor) ToDateLocal = floor;
		}

		// Ensure non-inverted range
		if (FromDateLocal > ToDateLocal) FromDateLocal = ToDateLocal;

		SelectedRange = "custom";
		await RefreshPreviewIfAuthenticatedAsync();
	}

	public string DownloadUrl(string kind)
	{
		var fromUtc = DateTime.SpecifyKind(FromDateLocal, DateTimeKind.Local).ToUniversalTime();
		var toUtc = DateTime.SpecifyKind(ToDateLocal.AddDays(1).AddSeconds(-1), DateTimeKind.Local).ToUniversalTime();
		return $"/api/export/{kind}.csv?from={fromUtc:o}&to={toUtc:o}";
	}

	public int RowCountFor(string kind) => kind switch
	{
		"revenue" => Preview?.RevenueRowCount ?? 0,
		"expenses" => Preview?.ExpenseRowCount ?? 0,
		"payouts" => Preview?.PayoutRowCount ?? 0,
		_ => 0
	};

	private async Task RefreshPreviewIfAuthenticatedAsync()
	{
		if (!CurrentUserService.IsAuthenticated || !CurrentUserService.UserId.HasValue)
			return;

		await RefreshPreviewAsync(CurrentUserService.UserId.Value);
	}

	private async Task RefreshPreviewAsync(long userId)
	{
		IsRefreshingPreview = true;
		try
		{
			var fromUtc = DateTime.SpecifyKind(FromDateLocal, DateTimeKind.Local).ToUniversalTime();
			var toUtc = DateTime.SpecifyKind(ToDateLocal.AddDays(1).AddSeconds(-1), DateTimeKind.Local).ToUniversalTime();
			Preview = await PreviewCommand.ExecuteAsync(userId, fromUtc, toUtc);
		}
		finally
		{
			IsRefreshingPreview = false;
		}
	}


	#endregion
}
