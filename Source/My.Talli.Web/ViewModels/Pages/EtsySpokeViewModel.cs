namespace My.Talli.Web.ViewModels.Pages;

using Domain.Data.Interfaces;
using Domain.Repositories;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Models;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>View Model</summary>
public class EtsySpokeViewModel : ComponentBase
{
	#region <Constants>

	private const string PlatformName = "Etsy";

	#endregion

	#region <Variables>

	private long? _userId;

	[CascadingParameter]
	private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

	[Inject]
	private ICurrentUserService CurrentUserService { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.Expense, ENTITIES.Expense> ExpenseAdapter { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.ExpenseEtsy, ENTITIES.ExpenseEtsy> ExpenseEtsyAdapter { get; set; } = default!;

	[Inject]
	private NavigationManager Navigation { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.Payout, ENTITIES.Payout> PayoutAdapter { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.PayoutEtsy, ENTITIES.PayoutEtsy> PayoutEtsyAdapter { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.PlatformConnection, ENTITIES.PlatformConnection> PlatformConnectionAdapter { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.Revenue, ENTITIES.Revenue> RevenueAdapter { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.RevenueEtsy, ENTITIES.RevenueEtsy> RevenueEtsyAdapter { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.ShopConnection, ENTITIES.ShopConnection> ShopConnectionAdapter { get; set; } = default!;

	#endregion

	#region <Properties>

	public string ActiveTab { get; private set; } = "revenue";

	public int CurrentPage { get; private set; } = 1;

	public List<EtsyExpenseItem> ExpenseItems { get; private set; } = [];

	public decimal Expenses30d => ExpenseItems.Where(e => e.ExpenseDate >= DateTime.UtcNow.AddDays(-30)).Sum(e => e.Amount);

	public string ExpenseCategoryFilter { get; private set; } = "all";

	public bool HasAnyConnectedShop => Shops.Count > 0;

	public bool IsLoading { get; private set; } = true;

	public DateTime? LastSyncDateTime { get; private set; }

	public decimal Net30d => RevenueItems.Where(r => r.TransactionDate >= DateTime.UtcNow.AddDays(-30)).Sum(r => r.NetAmount);

	public string PageTitle => ActiveTab switch
	{
		"overview" => "Etsy — Overview",
		"expenses" => "Etsy — Expenses",
		"payouts" => "Etsy — Payouts",
		_ => "Etsy — Revenue"
	};

	public const int PageSize = 25;

	public List<EtsyPayoutItem> PayoutItems { get; private set; } = [];

	public string PayoutStatusFilter { get; private set; } = "all";

	public string Period { get; private set; } = "30D";

	public List<EtsyRevenueItem> RevenueItems { get; private set; } = [];

	public long? SelectedShopId { get; private set; }

	public List<EtsyShopOption> Shops { get; private set; } = [];

	#endregion

	#region <Methods>

	public List<EtsyExpenseItem> GetPagedExpenseItems()
	{
		var filtered = GetVisibleExpenseItems();
		return filtered.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();
	}

	public List<EtsyPayoutItem> GetPagedPayoutItems()
	{
		var filtered = GetVisiblePayoutItems();
		return filtered.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();
	}

	public List<EtsyRevenueItem> GetPagedRevenueItems()
	{
		var filtered = GetVisibleRevenueItems();
		return filtered.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();
	}

	public int GetTotalPageCount()
	{
		var count = ActiveTab switch
		{
			"expenses" => GetVisibleExpenseItems().Count,
			"payouts" => GetVisiblePayoutItems().Count,
			_ => GetVisibleRevenueItems().Count
		};

		return Math.Max(1, (int)Math.Ceiling(count / (double)PageSize));
	}

	public List<EtsyExpenseItem> GetVisibleExpenseItems()
	{
		IEnumerable<EtsyExpenseItem> items = ExpenseItems;

		if (SelectedShopId.HasValue)
			items = items.Where(e => e.ShopConnectionId == SelectedShopId.Value);

		if (ExpenseCategoryFilter != "all")
		{
			items = ExpenseCategoryFilter switch
			{
				"listing" => items.Where(e => e.Category == "Listing Fee"),
				"ad" => items.Where(e => e.Category == "Ad Fee"),
				"subscription" => items.Where(e => e.Category == "Subscription Fee"),
				_ => items
			};
		}

		items = ApplyPeriodFilter(items, e => e.ExpenseDate);
		return items.OrderByDescending(e => e.ExpenseDate).ToList();
	}

	public List<EtsyPayoutItem> GetVisiblePayoutItems()
	{
		IEnumerable<EtsyPayoutItem> items = PayoutItems;

		if (SelectedShopId.HasValue)
			items = items.Where(p => p.ShopConnectionId == SelectedShopId.Value);

		if (PayoutStatusFilter != "all")
		{
			items = PayoutStatusFilter switch
			{
				"intransit" => items.Where(p => p.Status == "In Transit"),
				"paid" => items.Where(p => p.Status == "Paid"),
				_ => items
			};
		}

		return items.OrderByDescending(p => p.PayoutDate).ToList();
	}

	public List<EtsyRevenueItem> GetVisibleRevenueItems()
	{
		IEnumerable<EtsyRevenueItem> items = RevenueItems;

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

	public decimal OverviewExpenses30d => ExpenseItems.Where(e => e.ExpenseDate >= DateTime.UtcNow.AddDays(-30)).Sum(e => e.Amount);

	public decimal OverviewNet30d => RevenueItems.Where(r => r.TransactionDate >= DateTime.UtcNow.AddDays(-30) && !r.IsRefunded && !r.IsDisputed).Sum(r => r.NetAmount);

	public EtsyPayoutItem? OverviewNextPayout => PayoutItems.Where(p => p.Status == "In Transit").OrderBy(p => p.PayoutDate).FirstOrDefault();

	public static string GetRelativeTime(DateTime utc)
	{
		var delta = DateTime.UtcNow - utc;
		if (delta.TotalMinutes < 1) return "just now";
		if (delta.TotalMinutes < 60) return $"{(int)delta.TotalMinutes} minute{((int)delta.TotalMinutes == 1 ? "" : "s")} ago";
		if (delta.TotalHours < 24) return $"{(int)delta.TotalHours} hour{((int)delta.TotalHours == 1 ? "" : "s")} ago";
		return $"{(int)delta.TotalDays} day{((int)delta.TotalDays == 1 ? "" : "s")} ago";
	}

	public int PagedEnd(int totalCount) => Math.Min(CurrentPage * PageSize, totalCount);

	public int PagedStart() => (CurrentPage - 1) * PageSize + 1;

	public decimal OverviewRevenue30d => RevenueItems.Where(r => r.TransactionDate >= DateTime.UtcNow.AddDays(-30)).Sum(r => r.GrossAmount);

	public void SelectExpenseCategory(string category)
	{
		ExpenseCategoryFilter = category;
		CurrentPage = 1;
	}

	public void SelectPayoutStatus(string status)
	{
		PayoutStatusFilter = status;
		CurrentPage = 1;
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

	public IEnumerable<(string Title, long ListingId, decimal Revenue, int Count)> TopListings()
	{
		return RevenueItems
			.Where(r => !r.IsRefunded && !r.IsDisputed && r.ListingId > 0)
			.GroupBy(r => r.ListingId)
			.Select(g => (
				Title: g.OrderByDescending(r => r.TransactionDate).First().Description,
				ListingId: g.Key,
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
			await LoadExpensesAsync();
			await LoadPayoutsAsync();
			PopulateSaleCounts();
		}
		finally
		{
			IsLoading = false;
		}
	}

	private async Task LoadExpensesAsync()
	{
		var userId = _userId!.Value;
		var expenses = (await ExpenseAdapter.FindAsync(e => e.UserId == userId && e.Platform == PlatformName)).ToList();

		if (expenses.Count == 0)
		{
			ExpenseItems = [];
			return;
		}

		var expenseIds = expenses.Select(e => e.Id).ToList();
		var details = (await ExpenseEtsyAdapter.FindAsync(d => expenseIds.Contains(d.Id))).ToDictionary(d => d.Id);
		var shopLookup = Shops.ToDictionary(s => s.ShopConnectionId, s => s.ShopName);

		ExpenseItems = expenses.Select(e =>
		{
			details.TryGetValue(e.Id, out var detail);
			return new EtsyExpenseItem
			{
				AdCampaignId = detail?.AdCampaignId,
				Amount = e.Amount,
				Category = e.Category,
				Currency = e.Currency,
				Description = e.Description,
				ExpenseDate = e.ExpenseDate,
				Id = e.Id,
				LedgerEntryId = detail?.LedgerEntryId,
				ListingId = detail?.ListingId,
				ShopConnectionId = e.ShopConnectionId,
				ShopName = e.ShopConnectionId.HasValue && shopLookup.TryGetValue(e.ShopConnectionId.Value, out var name) ? name : "Etsy"
			};
		}).ToList();
	}

	private async Task LoadPayoutsAsync()
	{
		var userId = _userId!.Value;
		var payouts = (await PayoutAdapter.FindAsync(p => p.UserId == userId && p.Platform == PlatformName)).ToList();

		if (payouts.Count == 0)
		{
			PayoutItems = [];
			return;
		}

		var payoutIds = payouts.Select(p => p.Id).ToList();
		var details = (await PayoutEtsyAdapter.FindAsync(d => payoutIds.Contains(d.Id))).ToDictionary(d => d.Id);
		var shopLookup = Shops.ToDictionary(s => s.ShopConnectionId, s => s.ShopName);

		PayoutItems = payouts.Select(p =>
		{
			details.TryGetValue(p.Id, out var detail);
			return new EtsyPayoutItem
			{
				Amount = p.Amount,
				Currency = p.Currency,
				ExpectedArrivalDate = p.ExpectedArrivalDate,
				Id = p.Id,
				LedgerEntryId = detail?.LedgerEntryId,
				PayoutDate = p.PayoutDate,
				ShopConnectionId = p.ShopConnectionId,
				ShopName = p.ShopConnectionId.HasValue && shopLookup.TryGetValue(p.ShopConnectionId.Value, out var name) ? name : "Etsy",
				Status = p.Status
			};
		}).ToList();
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
		var details = (await RevenueEtsyAdapter.FindAsync(d => revenueIds.Contains(d.Id))).ToDictionary(d => d.Id);
		var shopLookup = Shops.ToDictionary(s => s.ShopConnectionId, s => s.ShopName);

		RevenueItems = revenues.Select(r =>
		{
			details.TryGetValue(r.Id, out var detail);
			return new EtsyRevenueItem
			{
				Currency = r.Currency,
				Description = r.Description,
				FeeAmount = r.FeeAmount,
				GrossAmount = r.GrossAmount,
				Id = r.Id,
				IsDisputed = r.IsDisputed,
				IsRefunded = r.IsRefunded,
				ListingId = detail?.ListingId ?? 0,
				NetAmount = r.NetAmount,
				ReceiptId = detail?.ReceiptId ?? 0,
				ShopConnectionId = r.ShopConnectionId,
				ShopName = r.ShopConnectionId.HasValue && shopLookup.TryGetValue(r.ShopConnectionId.Value, out var name) ? name : "Etsy",
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
			.Select(s => new EtsyShopOption
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

	#endregion
}
