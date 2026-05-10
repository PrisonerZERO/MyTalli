namespace My.Talli.Web.ViewModels.Pages;

using Domain.Components.JsonSerializers;
using Domain.Data.Interfaces;
using Domain.Repositories;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Models;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>View Model</summary>
public class GumroadSpokeViewModel : ComponentBase
{
	#region <Constants>

	private const string PlatformName = "Gumroad";

	#endregion

	#region <Variables>

	private long? _userId;

	[CascadingParameter]
	private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

	[Inject]
	private ICurrentUserService CurrentUserService { get; set; } = default!;

	[Inject]
	private NavigationManager Navigation { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.PlatformConnection, ENTITIES.PlatformConnection> PlatformConnectionAdapter { get; set; } = default!;

	[Inject]
	private UserPreferencesJsonSerializer PreferencesSerializer { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.Revenue, ENTITIES.Revenue> RevenueAdapter { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.RevenueGumroad, ENTITIES.RevenueGumroad> RevenueGumroadAdapter { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.ShopConnection, ENTITIES.ShopConnection> ShopConnectionAdapter { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.User, ENTITIES.User> UserAdapter { get; set; } = default!;

	#endregion

	#region <Properties>

	public string ActiveTab { get; private set; } = "revenue";

	public int CurrentPage { get; private set; } = 1;

	public string Density { get; private set; } = "compact";

	public string DensityCss => $"density-{Density}";

	public bool HasAnyConnectedShop => Shops.Count > 0;

	public bool IsLoading { get; private set; } = true;

	public DateTime? LastSyncDateTime { get; private set; }

	public decimal OverviewNet30d => RevenueItems.Where(r => r.TransactionDate >= DateTime.UtcNow.AddDays(-30) && !r.IsRefunded && !r.IsDisputed).Sum(r => r.NetAmount);

	public decimal OverviewRevenue30d => RevenueItems.Where(r => r.TransactionDate >= DateTime.UtcNow.AddDays(-30)).Sum(r => r.GrossAmount);

	public string PageTitle => ActiveTab switch
	{
		"overview" => "Gumroad — Overview",
		_ => "Gumroad — Revenue"
	};

	public int PageSize { get; set; } = 50;

	public int[] PageSizeOptions { get; } = [10, 25, 50];

	public string Period { get; private set; } = "30D";

	public string PeriodLabel => Period switch
	{
		"7D" => "last 7 days",
		"30D" => "last 30 days",
		"90D" => "last 90 days",
		_ => "all time",
	};

	public List<GumroadRevenueItem> RevenueItems { get; private set; } = [];

	public long? SelectedShopId { get; private set; }

	public List<GumroadShopOption> Shops { get; private set; } = [];

	#endregion

	#region <Methods>

	public async Task ChangePageSize(int newSize)
	{
		PageSize = newSize;
		CurrentPage = 1;
		await SaveGridPreferencesAsync();
	}

	public List<GumroadRevenueItem> GetPagedRevenueItems()
	{
		var filtered = GetVisibleRevenueItems();
		return filtered.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();
	}

	public int GetTotalPageCount()
	{
		var count = GetVisibleRevenueItems().Count;
		return Math.Max(1, (int)Math.Ceiling(count / (double)PageSize));
	}

	public List<GumroadRevenueItem> GetVisibleRevenueItems()
	{
		IEnumerable<GumroadRevenueItem> items = RevenueItems;

		if (SelectedShopId.HasValue)
			items = items.Where(r => r.ShopConnectionId == SelectedShopId.Value);

		items = ApplyPeriodFilter(items, r => r.TransactionDate);
		return items.OrderByDescending(r => r.TransactionDate).ToList();
	}

	public void GoToPage(int page)
	{
		var total = GetTotalPageCount();
		CurrentPage = Math.Clamp(page, 1, total);
	}

	public int PagedEnd(int totalCount) => Math.Min(CurrentPage * PageSize, totalCount);

	public int PagedStart() => (CurrentPage - 1) * PageSize + 1;

	public static string GetRelativeTime(DateTime utc)
	{
		var delta = DateTime.UtcNow - utc;
		if (delta.TotalMinutes < 1) return "just now";
		if (delta.TotalMinutes < 60) return $"{(int)delta.TotalMinutes} minute{((int)delta.TotalMinutes == 1 ? "" : "s")} ago";
		if (delta.TotalHours < 24) return $"{(int)delta.TotalHours} hour{((int)delta.TotalHours == 1 ? "" : "s")} ago";
		return $"{(int)delta.TotalDays} day{((int)delta.TotalDays == 1 ? "" : "s")} ago";
	}

	public void SelectPeriod(string period)
	{
		Period = period;
		CurrentPage = 1;
	}

	public void SelectShop(long? shopId)
	{
		SelectedShopId = shopId;
		CurrentPage = 1;
	}

	public void SelectTab(string tab)
	{
		ActiveTab = tab;
		CurrentPage = 1;
	}

	public async Task SetDensity(string density)
	{
		Density = density;
		await SaveGridPreferencesAsync();
	}

	public IEnumerable<(string Title, decimal Revenue, int Count)> TopProducts()
	{
		return RevenueItems
			.Where(r => !r.IsRefunded && !r.IsDisputed)
			.GroupBy(r => r.Description)
			.Select(g => (
				Title: g.Key,
				Revenue: g.Sum(r => r.NetAmount),
				Count: g.Count()))
			.OrderByDescending(t => t.Revenue)
			.Take(5);
	}

	protected override async Task OnInitializedAsync()
	{
		var authState = await AuthenticationStateTask;
		var userIdClaim = authState.User.FindFirst("UserId")?.Value;

		if (!long.TryParse(userIdClaim, out var parsedUserId))
		{
			Navigation.NavigateTo("/signin", true);
			return;
		}

		_userId = parsedUserId;
		CurrentUserService.Set(parsedUserId, string.Empty);

		await LoadGridPreferencesAsync();
		await LoadAsync();
	}

	private IEnumerable<T> ApplyPeriodFilter<T>(IEnumerable<T> source, Func<T, DateTime> dateSelector)
	{
		var cutoff = Period switch
		{
			"7D" => DateTime.UtcNow.AddDays(-7),
			"30D" => DateTime.UtcNow.AddDays(-30),
			"90D" => DateTime.UtcNow.AddDays(-90),
			_ => DateTime.MinValue
		};

		return cutoff == DateTime.MinValue ? source : source.Where(x => dateSelector(x) >= cutoff);
	}

	private async Task LoadAsync()
	{
		if (!_userId.HasValue)
			return;

		IsLoading = true;

		try
		{
			await LoadShopsAsync();
			await LoadRevenueAsync();
			PopulateSaleCounts();
		}
		finally
		{
			IsLoading = false;
		}
	}

	private async Task LoadGridPreferencesAsync()
	{
		if (_userId is null) return;

		var user = await UserAdapter.GetByIdAsync(_userId.Value);
		if (user is null) return;

		var preferences = PreferencesSerializer.Deserialize(user.UserPreferences);

		if (preferences.GridPreferences.TryGetValue("gumroad.grid", out var gridPrefs))
		{
			if (!string.IsNullOrEmpty(gridPrefs.Density)) Density = gridPrefs.Density;
			if (gridPrefs.PageSize > 0) PageSize = gridPrefs.PageSize;
		}
	}

	private async Task LoadRevenueAsync()
	{
		var userId = _userId!.Value;
		var revenues = (await RevenueAdapter.FindAsync(r => r.UserId == userId && r.Platform == PlatformName)).ToList();

		if (revenues.Count == 0)
		{
			RevenueItems = [];
			return;
		}

		var revenueIds = revenues.Select(r => r.Id).ToList();
		var details = (await RevenueGumroadAdapter.FindAsync(d => revenueIds.Contains(d.Id))).ToDictionary(d => d.Id);
		var shopLookup = Shops.ToDictionary(s => s.ShopConnectionId, s => s.ShopName);

		RevenueItems = revenues.Select(r =>
		{
			details.TryGetValue(r.Id, out var detail);
			return new GumroadRevenueItem
			{
				Currency = r.Currency,
				Description = r.Description,
				FeeAmount = r.FeeAmount,
				GrossAmount = r.GrossAmount,
				Id = r.Id,
				IsDisputed = r.IsDisputed,
				IsRefunded = r.IsRefunded,
				LicenseKey = detail?.LicenseKey,
				NetAmount = r.NetAmount,
				SaleId = detail?.SaleId ?? string.Empty,
				ShopConnectionId = r.ShopConnectionId,
				ShopName = r.ShopConnectionId.HasValue && shopLookup.TryGetValue(r.ShopConnectionId.Value, out var name) ? name : "Gumroad",
				TransactionDate = r.TransactionDate
			};
		}).ToList();
	}

	private async Task LoadShopsAsync()
	{
		var userId = _userId!.Value;
		var platformConnection = (await PlatformConnectionAdapter.FindAsync(p => p.UserId == userId && p.Platform == PlatformName)).FirstOrDefault();

		if (platformConnection is null)
		{
			Shops = [];
			LastSyncDateTime = null;
			return;
		}

		var shops = (await ShopConnectionAdapter.FindAsync(s => s.PlatformConnectionId == platformConnection.Id)).ToList();

		Shops = shops
			.OrderBy(s => s.ShopName)
			.Select(s => new GumroadShopOption
			{
				IsEnabled = s.IsEnabled,
				ShopConnectionId = s.Id,
				ShopName = string.IsNullOrEmpty(s.ShopName) ? "Unnamed shop" : s.ShopName
			})
			.ToList();

		LastSyncDateTime = shops.Where(s => s.LastSyncDateTime.HasValue).Max(s => s.LastSyncDateTime);
	}

	private void PopulateSaleCounts()
	{
		foreach (var shop in Shops)
			shop.SaleCount = RevenueItems.Count(r => r.ShopConnectionId == shop.ShopConnectionId);
	}

	private async Task SaveGridPreferencesAsync()
	{
		if (_userId is null) return;

		var user = await UserAdapter.GetByIdAsync(_userId.Value);
		if (user is null) return;

		var preferences = PreferencesSerializer.Deserialize(user.UserPreferences);

		preferences.GridPreferences["gumroad.grid"] = new MODELS.GridPreference
		{
			Density = Density,
			PageSize = PageSize,
			SortColumn = string.Empty,
			SortDescending = true,
		};

		user.UserPreferences = PreferencesSerializer.Serialize(preferences);
		await UserAdapter.UpdateAsync(user);
	}

	#endregion
}
