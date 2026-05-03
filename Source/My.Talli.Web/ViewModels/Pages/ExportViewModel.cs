namespace My.Talli.Web.ViewModels.Pages;

using Domain.Commands.Export;
using Domain.Data.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

/// <summary>View Model</summary>
public class ExportViewModel : ComponentBase
{
	#region <Variables>

	[CascadingParameter]
	private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

	[Inject]
	private ICurrentUserService CurrentUserService { get; set; } = default!;

	[Inject]
	private GetExportPreviewCommand PreviewCommand { get; set; } = default!;

	[Inject]
	private NavigationManager Navigation { get; set; } = default!;


	#endregion

	#region <Properties>

	public DateTime FromDateLocal { get; set; } = DateTime.Today.AddDays(-90);

	public bool IsLoading { get; private set; } = true;

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

		await RefreshPreviewAsync(userId);
		IsLoading = false;
	}


	#endregion

	#region <Methods>

	public async Task SelectRangeAsync(string range)
	{
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
