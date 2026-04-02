namespace My.Talli.Web.ViewModels.Pages;

using Domain.Components.JsonSerializers;
using Domain.Data.Interfaces;
using Domain.Framework;
using Domain.Repositories;
using Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;
using Models;
using Services.Identity;
using Services.UI;
using System.Security.Claims;
using System.Text;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>View Model</summary>
public class DashboardViewModel : ComponentBase
{
	#region <Variables>

	private long? _userId;

	[CascadingParameter]
	private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

	[Inject]
	private ICurrentUserService CurrentUserService { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.Expense, ENTITIES.Expense> ExpenseAdapter { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.PlatformConnection, ENTITIES.PlatformConnection> PlatformConnectionAdapter { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.Goal, ENTITIES.Goal> GoalAdapter { get; set; } = default!;

	[Inject]
	private IOptions<KnownIssueSettings> KnownIssueOptions { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.Payout, ENTITIES.Payout> PayoutAdapter { get; set; } = default!;

	[Inject]
	private UserPreferencesJsonSerializer PreferencesSerializer { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.Revenue, ENTITIES.Revenue> RevenueAdapter { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.Subscription, ENTITIES.Subscription> SubscriptionAdapter { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.User, ENTITIES.User> UserAdapter { get; set; } = default!;

	[Inject]
	private UserDisplayCache UserDisplayCache { get; set; } = default!;

	#endregion

	#region <Properties>

	public string ActivePeriod { get; private set; } = "30D";

	public string ActiveTab { get; private set; } = "overview";

	public string Density { get; private set; } = "comfortable";

	public string DensityCss => $"density-{Density}";

	public string ChartCurrentAreaPath { get; private set; } = string.Empty;

	public string ChartCurrentLinePath { get; private set; } = string.Empty;

	public string ChartPreviousAreaPath { get; private set; } = string.Empty;

	public string ChartPreviousLinePath { get; private set; } = string.Empty;

	public double CircleStrokeDasharray { get; } = 364.42;

	public double CircleStrokeDashoffset => CircleStrokeDasharray * (1 - GoalPercentage / 100.0);

	public int DaysRemaining { get; private set; } = 12;

	public int ExpenseCurrentPage { get; private set; } = 1;

	public List<ExpenseItem> Expenses { get; private set; } = [];

	public string ExpenseSortColumn { get; private set; } = "ExpenseDate";

	public bool ExpenseSortDescending { get; private set; } = true;

	public decimal GoalCurrentAmount { get; private set; } = 1847m;

	public bool GoalOnTrack { get; private set; } = true;

	public int GoalPercentage { get; private set; } = 68;

	public decimal GoalTargetAmount { get; private set; } = 2700m;

	public bool HasGoal { get; private set; } = true;

	public bool IsSampleData { get; private set; } = true;

	public string KnownIssueMessage { get; private set; } = string.Empty;

	public string KnownIssueSeverity { get; private set; } = "Warning";

	public bool ShowKnownIssue { get; private set; }

	public int MonthlyGoalPercentage { get; private set; } = 68;

	public List<ExpenseItem> PagedExpenses => GetSortedExpenses().Skip((ExpenseCurrentPage - 1) * PageSize).Take(PageSize).ToList();

	public List<PayoutItem> PagedPayouts => GetSortedPayouts().Skip((PayoutCurrentPage - 1) * PageSize).Take(PageSize).ToList();

	public List<Transaction> PagedRevenueTransactions => GetSortedRevenue().Skip((RevenueCurrentPage - 1) * PageSize).Take(PageSize).ToList();

	public int PageSize { get; set; } = 10;

	public int[] PageSizeOptions { get; } = [10, 25, 50];

	public string PageTitle => ActiveTab == "overview" ? "Dashboard" : $"Dashboard — {ActiveTab[0].ToString().ToUpper()}{ActiveTab[1..]}";

	public int PayoutCurrentPage { get; private set; } = 1;

	public List<PayoutItem> Payouts { get; private set; } = [];

	public string PayoutSortColumn { get; private set; } = "PayoutDate";

	public bool PayoutSortDescending { get; private set; } = true;

	public List<string> Periods { get; } = ["7D", "30D", "90D", "12M"];

	public List<PlatformBreakdown> Platforms { get; private set; } = [];

	public int PlatformsConnected { get; private set; } = 3;

	public List<Transaction> RecentTransactions { get; private set; } = [];

	public int RevenueCurrentPage { get; private set; } = 1;

	public string RevenueSortColumn { get; private set; } = "TransactionDate";

	public bool RevenueSortDescending { get; private set; } = true;

	public string ThisMonthChange { get; private set; } = "12%";

	public decimal ThisMonthRevenue { get; private set; } = 1847m;

	public decimal TotalRevenue { get; private set; } = 4218m;

	public int TotalExpensePages => Math.Max(1, (int)Math.Ceiling((double)Expenses.Count / PageSize));

	public int TotalPayoutPages => Math.Max(1, (int)Math.Ceiling((double)Payouts.Count / PageSize));

	public string TotalRevenueChange { get; private set; } = "23%";

	public int TotalRevenuePages => Math.Max(1, (int)Math.Ceiling((double)RecentTransactions.Count / PageSize));

	public string UserFirstName { get; private set; } = string.Empty;


	#endregion

	#region <Events>

	protected override async Task OnInitializedAsync()
	{
		LoadKnownIssue();
		await LoadUserFromClaims();

		if (IsSampleData)
			LoadMockData();
		else
			await LoadRealDataAsync();
	}


	#endregion

	#region <Methods>

	public async Task SelectPeriod(string period)
	{
		ActivePeriod = period;

		if (!IsSampleData)
			await LoadRealDataAsync();
	}

	public void SelectTab(string tab)
	{
		ActiveTab = tab;
	}

	// ── Grid controls ──

	public async Task ChangePageSize()
	{
		RevenueCurrentPage = 1;
		ExpenseCurrentPage = 1;
		PayoutCurrentPage = 1;
		await SaveGridPreferencesAsync();
	}

	public void ExpenseGoToPage(int page)
	{
		ExpenseCurrentPage = Math.Clamp(page, 1, TotalExpensePages);
	}

	public async Task ExpenseSortBy(string column)
	{
		if (ExpenseSortColumn == column)
		{
			ExpenseSortDescending = !ExpenseSortDescending;
		}
		else
		{
			ExpenseSortColumn = column;
			ExpenseSortDescending = column == "ExpenseDate";
		}

		ExpenseCurrentPage = 1;
		await SaveGridPreferencesAsync();
	}

	public string GetExpenseSortIndicator(string column) => ExpenseSortColumn == column ? (ExpenseSortDescending ? "▼" : "▲") : "";

	public string GetPayoutSortIndicator(string column) => PayoutSortColumn == column ? (PayoutSortDescending ? "▼" : "▲") : "";

	public string GetRevenueSortIndicator(string column) => RevenueSortColumn == column ? (RevenueSortDescending ? "▼" : "▲") : "";

	public void PayoutGoToPage(int page)
	{
		PayoutCurrentPage = Math.Clamp(page, 1, TotalPayoutPages);
	}

	public async Task PayoutSortBy(string column)
	{
		if (PayoutSortColumn == column)
		{
			PayoutSortDescending = !PayoutSortDescending;
		}
		else
		{
			PayoutSortColumn = column;
			PayoutSortDescending = column == "PayoutDate";
		}

		PayoutCurrentPage = 1;
		await SaveGridPreferencesAsync();
	}

	public void RevenueGoToPage(int page)
	{
		RevenueCurrentPage = Math.Clamp(page, 1, TotalRevenuePages);
	}

	public async Task RevenueSortBy(string column)
	{
		if (RevenueSortColumn == column)
		{
			RevenueSortDescending = !RevenueSortDescending;
		}
		else
		{
			RevenueSortColumn = column;
			RevenueSortDescending = column == "TransactionDate";
		}

		RevenueCurrentPage = 1;
		await SaveGridPreferencesAsync();
	}

	public async Task SetDensity(string density)
	{
		Density = density;
		await SaveGridPreferencesAsync();
	}

	// ── Chart helpers ──

	private static string BuildChartPath(List<(DateTime Date, decimal Amount)> dailyTotals, DateTime start, DateTime end, decimal yMax)
	{
		if (dailyTotals.Count == 0)
			return "M40,200 L796,200";

		if (yMax <= 0) yMax = 1;

		var totalDays = Math.Max(1, (end - start).Days);
		var sb = new StringBuilder();

		for (var i = 0; i < dailyTotals.Count; i++)
		{
			var (date, amount) = dailyTotals[i];
			var dayOffset = (date - start).Days;
			var x = 40 + (int)(756.0 * dayOffset / totalDays);
			var y = (int)(200 - 170.0 * (double)(amount / yMax));
			y = Math.Clamp(y, 5, 200);

			sb.Append(i == 0 ? $"M{x},{y}" : $" L{x},{y}");
		}

		return sb.ToString();
	}

	private static string FormatChange(decimal current, decimal previous)
	{
		if (previous == 0)
			return current > 0 ? "+100%" : "0%";

		var pct = (current - previous) / previous * 100;
		var sign = pct >= 0 ? "+" : "";
		return $"{sign}{pct:F0}%";
	}

	private static string GetPlatformColor(string platform) => platform switch
	{
		"Stripe" => "#635bff",
		"Etsy" => "#f56400",
		"Gumroad" => "#ff90e8",
		"PayPal" => "#003087",
		"Shopify" => "#96bf48",
		"Manual" => "#a78bfa",
		_ => "#999"
	};

	private static (DateTime start, DateTime end, DateTime prevStart, DateTime prevEnd) GetPeriodDateRange(string period)
	{
		var end = DateTime.Today;
		var days = period switch
		{
			"7D" => 7,
			"30D" => 30,
			"90D" => 90,
			"12M" => 365,
			_ => 30
		};

		var start = end.AddDays(-days);
		var prevEnd = start.AddDays(-1);
		var prevStart = prevEnd.AddDays(-days);

		return (start, end, prevStart, prevEnd);
	}

	private void LoadKnownIssue()
	{
		var settings = KnownIssueOptions.Value;
		ShowKnownIssue = settings.IsActive && !string.IsNullOrWhiteSpace(settings.Message);
		KnownIssueMessage = settings.Message;
		KnownIssueSeverity = settings.Severity;
	}

	private void LoadMockData()
	{
		ChartCurrentLinePath = DashboardDataset.GetChartCurrentLinePath();
		ChartCurrentAreaPath = ChartCurrentLinePath + " L796,200 L40,200 Z";
		ChartPreviousLinePath = DashboardDataset.GetChartPreviousLinePath();
		ChartPreviousAreaPath = ChartPreviousLinePath + " L796,200 L40,200 Z";
		Platforms = DashboardDataset.GetPlatforms();
		RecentTransactions = DashboardDataset.GetRecentTransactions();
		Expenses = ExpenseDataset.GetDashboardExpenses();
		Payouts = PayoutDataset.GetDashboardPayouts();
	}

	private async Task LoadRealDataAsync()
	{
		var (start, end, prevStart, prevEnd) = GetPeriodDateRange(ActivePeriod);
		var endInclusive = end.AddDays(1);
		var prevEndInclusive = prevEnd.AddDays(1);

		// Revenue for current and previous periods
		var allRevenues = await RevenueAdapter.FindAsync(r => r.UserId == _userId!.Value);
		var currentRevenues = allRevenues.Where(r => r.TransactionDate >= start && r.TransactionDate < endInclusive).ToList();
		var previousRevenues = allRevenues.Where(r => r.TransactionDate >= prevStart && r.TransactionDate < prevEndInclusive).ToList();

		// Summary cards
		TotalRevenue = currentRevenues.Sum(r => r.NetAmount);
		var previousTotal = previousRevenues.Sum(r => r.NetAmount);
		TotalRevenueChange = FormatChange(TotalRevenue, previousTotal);

		var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
		var prevMonthStart = monthStart.AddMonths(-1);
		ThisMonthRevenue = allRevenues.Where(r => r.TransactionDate >= monthStart).Sum(r => r.NetAmount);
		var prevMonthRevenue = allRevenues.Where(r => r.TransactionDate >= prevMonthStart && r.TransactionDate < monthStart).Sum(r => r.NetAmount);
		ThisMonthChange = FormatChange(ThisMonthRevenue, prevMonthRevenue);

		// Platform breakdown
		var platformGroups = currentRevenues.GroupBy(r => r.Platform).Select(g => new { Platform = g.Key, Total = g.Sum(r => r.NetAmount) }).OrderByDescending(g => g.Total).ToList();
		var platformTotal = platformGroups.Sum(g => g.Total);
		Platforms = platformGroups.Select(g => new PlatformBreakdown(g.Platform, GetPlatformColor(g.Platform), g.Total, platformTotal > 0 ? (int)(g.Total / platformTotal * 100) : 0)).ToList();
		PlatformsConnected = Platforms.Count;

		// Chart
		var currentDaily = currentRevenues.GroupBy(r => r.TransactionDate.Date).Select(g => (Date: g.Key, Amount: g.Sum(r => r.NetAmount))).OrderBy(d => d.Date).ToList();
		var previousDaily = previousRevenues.GroupBy(r => r.TransactionDate.Date).Select(g => (Date: g.Key, Amount: g.Sum(r => r.NetAmount))).OrderBy(d => d.Date).ToList();
		var yMax = Math.Max(currentDaily.Any() ? currentDaily.Max(d => d.Amount) : 0, previousDaily.Any() ? previousDaily.Max(d => d.Amount) : 0);

		ChartCurrentLinePath = BuildChartPath(currentDaily, start, end, yMax);
		ChartCurrentAreaPath = ChartCurrentLinePath + " L796,200 L40,200 Z";
		ChartPreviousLinePath = BuildChartPath(previousDaily, prevStart, prevEnd, yMax);
		ChartPreviousAreaPath = ChartPreviousLinePath + " L796,200 L40,200 Z";

		// All transactions (Revenue tab pages through these; Overview shows top 5)
		RecentTransactions = currentRevenues.OrderByDescending(r => r.TransactionDate).Select(r => new Transaction(r.Platform, GetPlatformColor(r.Platform), r.Description, r.TransactionDate.ToString("MMM d"), r.NetAmount, r.TransactionDate)).ToList();

		// Expenses
		var expenses = await ExpenseAdapter.FindAsync(e => e.UserId == _userId!.Value);
		Expenses = expenses.Select(ToExpenseItem).ToList();

		// Payouts
		var payouts = await PayoutAdapter.FindAsync(p => p.UserId == _userId!.Value);
		Payouts = payouts.Select(ToPayoutItem).ToList();

		// Goals
		await LoadGoalDataAsync();
	}

	private async Task LoadGoalDataAsync()
	{
		var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
		var monthEnd = monthStart.AddMonths(1).AddDays(-1);
		var today = DateTime.Today;

		// Find any active goal covering this month
		var goals = await GoalAdapter.FindAsync(g =>
			g.UserId == _userId!.Value &&
			g.StartDate <= monthEnd &&
			(g.EndDate == null || g.EndDate >= monthStart));

		var goal = goals.FirstOrDefault();

		if (goal is null)
		{
			HasGoal = false;
			GoalTargetAmount = 0;
			GoalCurrentAmount = 0;
			GoalPercentage = 0;
			MonthlyGoalPercentage = 0;
			DaysRemaining = 0;
			GoalOnTrack = false;
			return;
		}

		// Compute earned from revenue using the goal's date range + platform filter
		var allRevenues = await RevenueAdapter.FindAsync(r => r.UserId == _userId!.Value);
		var matchingRevenues = allRevenues.Where(r => r.TransactionDate >= goal.StartDate);

		if (goal.EndDate.HasValue)
			matchingRevenues = matchingRevenues.Where(r => r.TransactionDate <= goal.EndDate.Value);

		if (!string.IsNullOrEmpty(goal.Platform))
			matchingRevenues = matchingRevenues.Where(r => r.Platform == goal.Platform);

		var earned = matchingRevenues.Sum(r => r.NetAmount);

		HasGoal = true;
		GoalTargetAmount = goal.TargetAmount;
		GoalCurrentAmount = earned;
		GoalPercentage = GoalTargetAmount > 0 ? Math.Min(100, (int)(GoalCurrentAmount / GoalTargetAmount * 100)) : 0;
		MonthlyGoalPercentage = GoalPercentage;

		var effectiveEnd = goal.EndDate ?? monthEnd;
		DaysRemaining = Math.Max(0, (effectiveEnd - today).Days);

		// On track = projected pace meets target
		var daysElapsed = Math.Max(1, (today - goal.StartDate).Days + 1);
		var totalDays = Math.Max(1, (effectiveEnd - goal.StartDate).Days + 1);
		var projectedAmount = GoalCurrentAmount / daysElapsed * totalDays;
		GoalOnTrack = projectedAmount >= GoalTargetAmount;
	}

	private async Task LoadUserFromClaims()
	{
		var authState = await AuthenticationStateTask;
		var principal = authState.User;

		if (principal.Identity?.IsAuthenticated != true)
			return;

		var email = principal.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
		var userIdClaim = principal.FindFirst("UserId")?.Value;

		if (userIdClaim is null || !long.TryParse(userIdClaim, out var userId))
			return;

		_userId = userId;
		CurrentUserService.Set(userId, string.Empty);

		var (info, userPreferences) = await UserDisplayCache.GetOrLoadAsync(userId, email);

		UserFirstName = info.FirstName;

		var preferences = PreferencesSerializer.Deserialize(userPreferences);

		if (preferences.FunGreetings)
			UserFirstName = UserClaimsHelper.RandomFunGreeting();

		// Load saved grid preferences
		if (preferences.GridPreferences.TryGetValue("dashboard.revenueGrid", out var gridPrefs))
		{
			Density = gridPrefs.Density;
			PageSize = gridPrefs.PageSize;
			RevenueSortColumn = string.IsNullOrEmpty(gridPrefs.SortColumn) ? "TransactionDate" : gridPrefs.SortColumn;
			RevenueSortDescending = gridPrefs.SortDescending;
		}

		// Check for data sources: modules (ProductId >= 3) or platforms (not yet implemented)
		var moduleSubscriptions = await SubscriptionAdapter.FindAsync(s =>
			s.UserId == userId &&
			s.ProductId >= 3 &&
			(s.Status == SubscriptionStatuses.Active || s.Status == SubscriptionStatuses.Cancelling));

		var hasModules = moduleSubscriptions.Any();
		var platformConnections = await PlatformConnectionAdapter.FindAsync(p => p.UserId == userId);
		var hasPlatforms = platformConnections.Any();

		IsSampleData = !hasModules && !hasPlatforms;
	}

	private IEnumerable<ExpenseItem> GetSortedExpenses()
	{
		return ExpenseSortColumn switch
		{
			"Description" => ExpenseSortDescending ? Expenses.OrderByDescending(e => e.Description, StringComparer.OrdinalIgnoreCase) : Expenses.OrderBy(e => e.Description, StringComparer.OrdinalIgnoreCase),
			"Category" => ExpenseSortDescending ? Expenses.OrderByDescending(e => e.Category) : Expenses.OrderBy(e => e.Category),
			"Amount" => ExpenseSortDescending ? Expenses.OrderByDescending(e => e.Amount) : Expenses.OrderBy(e => e.Amount),
			"Platform" => ExpenseSortDescending ? Expenses.OrderByDescending(e => e.Platform) : Expenses.OrderBy(e => e.Platform),
			_ => ExpenseSortDescending ? Expenses.OrderByDescending(e => e.ExpenseDate) : Expenses.OrderBy(e => e.ExpenseDate),
		};
	}

	private IEnumerable<PayoutItem> GetSortedPayouts()
	{
		return PayoutSortColumn switch
		{
			"Amount" => PayoutSortDescending ? Payouts.OrderByDescending(p => p.Amount) : Payouts.OrderBy(p => p.Amount),
			"Status" => PayoutSortDescending ? Payouts.OrderByDescending(p => p.Status) : Payouts.OrderBy(p => p.Status),
			"Platform" => PayoutSortDescending ? Payouts.OrderByDescending(p => p.Platform) : Payouts.OrderBy(p => p.Platform),
			"ExpectedArrival" => PayoutSortDescending ? Payouts.OrderByDescending(p => p.ExpectedArrivalDate) : Payouts.OrderBy(p => p.ExpectedArrivalDate),
			_ => PayoutSortDescending ? Payouts.OrderByDescending(p => p.PayoutDate) : Payouts.OrderBy(p => p.PayoutDate),
		};
	}

	private IEnumerable<Transaction> GetSortedRevenue()
	{
		return RevenueSortColumn switch
		{
			"Description" => RevenueSortDescending ? RecentTransactions.OrderByDescending(t => t.Description, StringComparer.OrdinalIgnoreCase) : RecentTransactions.OrderBy(t => t.Description, StringComparer.OrdinalIgnoreCase),
			"Amount" => RevenueSortDescending ? RecentTransactions.OrderByDescending(t => t.Amount) : RecentTransactions.OrderBy(t => t.Amount),
			"Platform" => RevenueSortDescending ? RecentTransactions.OrderByDescending(t => t.PlatformName) : RecentTransactions.OrderBy(t => t.PlatformName),
			_ => RevenueSortDescending ? RecentTransactions.OrderByDescending(t => t.SortDate) : RecentTransactions.OrderBy(t => t.SortDate),
		};
	}

	private async Task SaveGridPreferencesAsync()
	{
		if (_userId is null) return;

		var user = await UserAdapter.GetByIdAsync(_userId.Value);
		if (user is null) return;

		var preferences = PreferencesSerializer.Deserialize(user.UserPreferences);

		preferences.GridPreferences["dashboard.revenueGrid"] = new MODELS.GridPreference
		{
			Density = Density,
			PageSize = PageSize,
			SortColumn = RevenueSortColumn,
			SortDescending = RevenueSortDescending,
		};

		user.UserPreferences = PreferencesSerializer.Serialize(preferences);
		await UserAdapter.UpdateAsync(user);
	}

	private ExpenseItem ToExpenseItem(MODELS.Expense expense)
	{
		return new ExpenseItem
		{
			Amount = expense.Amount,
			Category = expense.Category,
			Currency = expense.Currency,
			Description = expense.Description,
			ExpenseDate = expense.ExpenseDate,
			Id = expense.Id,
			Platform = expense.Platform,
		};
	}

	private PayoutItem ToPayoutItem(MODELS.Payout payout)
	{
		return new PayoutItem
		{
			Amount = payout.Amount,
			Currency = payout.Currency,
			ExpectedArrivalDate = payout.ExpectedArrivalDate,
			Id = payout.Id,
			PayoutDate = payout.PayoutDate,
			Platform = payout.Platform,
			Status = payout.Status,
		};
	}


	#endregion
}
