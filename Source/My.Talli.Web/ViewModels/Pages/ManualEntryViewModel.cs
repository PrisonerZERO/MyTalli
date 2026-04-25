namespace My.Talli.Web.ViewModels.Pages;

using Domain.Commands.Platforms;
using Domain.Components.JsonSerializers;
using Domain.Data.Interfaces;
using Domain.enums;
using Domain.extensions;
using Domain.Framework;
using Domain.Repositories;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Models;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>View Model</summary>
public class ManualEntryViewModel : ComponentBase
{
	#region <Variables>

	private long? _userId;

	[CascadingParameter]
	private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

	[Inject]
	private CreateManualShopCommand CreateManualShopCommand { get; set; } = default!;

	[Inject]
	private ICurrentUserService CurrentUserService { get; set; } = default!;

	[Inject]
	private RenameManualShopCommand RenameManualShopCommand { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.Expense, ENTITIES.Expense> ExpenseAdapter { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.Payout, ENTITIES.Payout> PayoutAdapter { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.PlatformConnection, ENTITIES.PlatformConnection> PlatformConnectionAdapter { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.Revenue, ENTITIES.Revenue> RevenueAdapter { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.RevenueManual, ENTITIES.RevenueManual> RevenueManualAdapter { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.ShopConnection, ENTITIES.ShopConnection> ShopConnectionAdapter { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.Subscription, ENTITIES.Subscription> SubscriptionAdapter { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.User, ENTITIES.User> UserAdapter { get; set; } = default!;

	[Inject]
	private UserPreferencesJsonSerializer PreferencesSerializer { get; set; } = default!;

	#endregion

	#region <Properties>

	public long? ActiveShopConnectionId { get; private set; }

	public string ActiveShopName => Shops.FirstOrDefault(s => s.Id == ActiveShopConnectionId)?.ShopName ?? "My Manual Shop";

	public string ActiveTab { get; private set; } = "revenue";

	public List<string> Categories { get; private set; } =
	[
		"Sale",
		"Service",
		"Freelance",
		"Consulting",
		"Digital Product",
		"Physical Product",
		"Other"
	];

	public int CurrentPage { get; private set; } = 1;

	public long? DeletingExpenseId { get; private set; }

	public string DeletingExpenseDescription => Expenses.FirstOrDefault(e => e.Id == DeletingExpenseId)?.Description ?? "";

	public long? DeletingId { get; private set; }

	public long? DeletingPayoutId { get; private set; }

	public string DeletingPayoutDescription => DeletingPayoutId.HasValue ? Payouts.FirstOrDefault(p => p.Id == DeletingPayoutId)?.Amount.ToString("C2") ?? "" : "";

	public string Density { get; private set; } = "comfortable";

	// Revenue inline edit fields
	public string EditCategory { get; set; } = "Sale";

	public string EditCurrency { get; set; } = "USD";

	public string EditDescription { get; set; } = string.Empty;

	public decimal EditExpenseAmount { get; set; }

	public string EditExpenseCategory { get; set; } = "Other";

	public DateTime EditExpenseDate { get; set; } = DateTime.Today;

	public string EditExpenseDescription { get; set; } = string.Empty;

	public decimal EditFeeAmount { get; set; }

	public long? EditingId { get; private set; }

	public long? EditingShopId { get; private set; }

	public string EditNotes { get; set; } = string.Empty;

	public decimal EditPayoutAmount { get; set; }

	public DateTime EditPayoutDate { get; set; } = DateTime.Today;

	public DateTime? EditPayoutExpectedArrival { get; set; }

	public string EditPayoutStatus { get; set; } = "Pending";

	public int EditQuantity { get; set; } = 1;

	public string EditShopName { get; set; } = string.Empty;

	public DateTime EditTransactionDate { get; set; } = DateTime.Today;

	public decimal EditUnitPrice { get; set; }

	public List<ManualEntryItem> Entries { get; private set; } = [];

	public List<string> ExpenseCategories { get; private set; } =
		Enum.GetValues<ExpenseCategory>().Select(c => c.ToStringValue()).ToList();

	public int ExpenseCurrentPage { get; private set; } = 1;

	public long? ExpenseEditingId { get; private set; }

	public List<ExpenseItem> Expenses { get; private set; } = [];

	public string ExpenseSortColumn { get; private set; } = "ExpenseDate";

	public bool ExpenseSortDescending { get; private set; } = true;

	public bool HasModuleAccess { get; private set; }

	public bool IsAddingShop { get; private set; }

	public bool IsSampleData { get; private set; }

	public bool IsShopDropdownOpen { get; private set; }

	public string NewShopName { get; set; } = string.Empty;

	// Revenue quick-entry fields
	public string NewCategory { get; set; } = "Sale";

	public string NewCurrency { get; set; } = "USD";

	public string NewDescription { get; set; } = string.Empty;

	// Expense quick-entry fields
	public decimal NewExpenseAmount { get; set; }

	public string NewExpenseCategory { get; set; } = "Other";

	public DateTime NewExpenseDate { get; set; } = DateTime.Today;

	public string NewExpenseDescription { get; set; } = string.Empty;

	public decimal NewFeeAmount { get; set; }

	public string NewNotes { get; set; } = string.Empty;

	// Payout quick-entry fields
	public decimal NewPayoutAmount { get; set; }

	public DateTime NewPayoutDate { get; set; } = DateTime.Today;

	public string NewPayoutStatus { get; set; } = "Pending";

	public int NewQuantity { get; set; } = 1;

	public DateTime NewTransactionDate { get; set; } = DateTime.Now;

	public decimal NewUnitPrice { get; set; }

	public List<ExpenseItem> PagedExpenses => GetSortedExpenses()
		.Skip((ExpenseCurrentPage - 1) * PageSize)
		.Take(PageSize)
		.ToList();

	public List<ManualEntryItem> PagedEntries => GetSortedEntries()
		.Skip((CurrentPage - 1) * PageSize)
		.Take(PageSize)
		.ToList();

	public List<PayoutItem> PagedPayouts => GetSortedPayouts()
		.Skip((PayoutCurrentPage - 1) * PageSize)
		.Take(PageSize)
		.ToList();

	public string PageTitle => ActiveTab == "revenue" ? "Manual Entry" : $"Manual Entry — {ActiveTab[0].ToString().ToUpper()}{ActiveTab[1..]}";

	public int PageSize { get; set; } = 10;

	public int[] PageSizeOptions { get; } = [10, 25, 50];

	public int PayoutCurrentPage { get; private set; } = 1;

	public long? PayoutEditingId { get; private set; }

	public List<PayoutItem> Payouts { get; private set; } = [];

	public string PayoutSortColumn { get; private set; } = "PayoutDate";

	public bool PayoutSortDescending { get; private set; } = true;

	public List<string> PayoutStatuses { get; private set; } =
	[
		"Pending",
		"In Transit",
		"Paid",
		"Failed",
		"Cancelled"
	];

	public List<MODELS.ShopConnection> Shops { get; private set; } = [];

	public bool ShowEditNotes { get; private set; }

	public string SortColumn { get; private set; } = "TransactionDate";

	public bool SortDescending { get; private set; } = true;

	public int TotalEntries => Entries.Count;

	public int TotalExpensePages => Math.Max(1, (int)Math.Ceiling((double)Expenses.Count / PageSize));

	public decimal TotalExpenses => Expenses.Sum(e => e.Amount);

	public decimal TotalGross => Entries.Sum(e => e.GrossAmount);

	public decimal TotalNet => Entries.Sum(e => e.NetAmount);

	public decimal TotalNetProfit => TotalNet - TotalExpenses;

	public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)Entries.Count / PageSize));

	public int TotalPayoutPages => Math.Max(1, (int)Math.Ceiling((double)Payouts.Count / PageSize));

	public decimal TotalPayouts => Payouts.Sum(p => p.Amount);

	protected ElementReference EditShopNameRef;

	protected ElementReference ExpenseQuickEntryDescriptionRef;

	protected ElementReference NewShopNameRef;

	protected ElementReference PayoutQuickEntryAmountRef;

	protected ElementReference QuickEntryDescriptionRef;

	#endregion

	#region <Events>

	protected override async Task OnInitializedAsync()
	{
		var authState = await AuthenticationStateTask;
		var principal = authState.User;

		if (principal.Identity?.IsAuthenticated != true)
			return;

		var userIdClaim = principal.FindFirst("UserId")?.Value;
		if (userIdClaim is null || !long.TryParse(userIdClaim, out var userId))
			return;

		_userId = userId;
		CurrentUserService.Set(userId, string.Empty);

		// Check module access: active subscription for Manual Entry Module (ProductId = 3)
		var subscriptions = await SubscriptionAdapter.FindAsync(s =>
			s.UserId == userId &&
			s.ProductId == 3 &&
			(s.Status == SubscriptionStatuses.Active || s.Status == SubscriptionStatuses.Cancelling));

		HasModuleAccess = subscriptions.Any();

		// Load saved grid preferences
		var user = await UserAdapter.GetByIdAsync(userId);
		if (user is not null)
		{
			var preferences = PreferencesSerializer.Deserialize(user.UserPreferences);
			if (preferences.GridPreferences.TryGetValue("manualEntry.entryGrid", out var gridPrefs))
			{
				Density = gridPrefs.Density;
				PageSize = gridPrefs.PageSize;
				SortColumn = string.IsNullOrEmpty(gridPrefs.SortColumn) ? "TransactionDate" : gridPrefs.SortColumn;
				SortDescending = gridPrefs.SortDescending;

				// Migrate old column name
				if (SortColumn == "GrossAmount") SortColumn = "UnitPrice";
			}
		}

		if (HasModuleAccess)
		{
			await LoadShopsAsync();
			await LoadEntriesAsync();
			await LoadExpensesAsync();
			await LoadPayoutsAsync();
		}
		else
		{
			IsSampleData = true;
			Entries = ManualEntryDataset.GetEntries();
			Expenses = ExpenseDataset.GetManualExpenses();
			Payouts = PayoutDataset.GetManualPayouts();
		}
	}

	#endregion

	#region <Methods>

	// ── Revenue CRUD ──

	public void CancelDelete()
	{
		DeletingId = null;
	}

	public void CancelEdit()
	{
		EditingId = null;
		ShowEditNotes = false;
	}

	public async Task ChangePageSize()
	{
		CurrentPage = 1;
		ExpenseCurrentPage = 1;
		PayoutCurrentPage = 1;
		await SaveGridPreferencesAsync();
	}

	public async Task ConfirmDeleteAsync()
	{
		if (DeletingId is null) return;

		var revenueId = DeletingId.Value;
		DeletingId = null;

		var revenue = await RevenueAdapter.GetByIdAsync(revenueId);
		if (revenue is null) return;

		var manualEntries = await RevenueManualAdapter.FindAsync(m => m.Id == revenueId);
		var manual = manualEntries.FirstOrDefault();

		if (manual is not null)
			await RevenueManualAdapter.DeleteAsync(manual);

		await RevenueAdapter.DeleteAsync(revenue);
		await LoadEntriesAsync();
	}

	public void DeleteEntry(long revenueId)
	{
		DeletingId = revenueId;
	}

	public string DeletingDescription => Entries.FirstOrDefault(e => e.Id == DeletingId)?.Description ?? "";

	public string DensityCss => $"density-{Density}";

	public string GetSortIndicator(string column) => SortColumn == column ? (SortDescending ? "▼" : "▲") : "";

	public void GoToPage(int page)
	{
		CurrentPage = Math.Clamp(page, 1, TotalPages);
	}

	public async Task HandleEditKeyDown(KeyboardEventArgs e)
	{
		if (e.Key == "Enter")
			await SaveEditAsync();
		else if (e.Key == "Escape")
			CancelEdit();
	}

	public async Task HandleQuickEntryKeyDown(KeyboardEventArgs e)
	{
		if (e.Key == "Enter")
			await QuickAddAsync();
	}

	public async Task QuickAddAsync()
	{
		var grossAmount = NewUnitPrice * NewQuantity;
		if (_userId is null || string.IsNullOrWhiteSpace(NewDescription) || grossAmount <= 0)
			return;

		var revenue = ToNewRevenue(grossAmount);
		revenue = await RevenueAdapter.InsertAsync(revenue);

		var manual = ToNewRevenueManual(revenue.Id);
		await RevenueManualAdapter.InsertAsync(manual);

		ResetQuickEntry();
		await LoadEntriesAsync();

		try { await QuickEntryDescriptionRef.FocusAsync(); } catch { /* element may not be rendered yet */ }
	}

	public async Task SaveEditAsync()
	{
		if (EditingId is null || _userId is null) return;

		var grossAmount = EditUnitPrice * EditQuantity;
		if (string.IsNullOrWhiteSpace(EditDescription) || grossAmount <= 0)
			return;

		var revenue = await RevenueAdapter.GetByIdAsync(EditingId.Value);
		if (revenue is null) return;

		revenue.Currency = EditCurrency;
		revenue.Description = EditDescription;
		revenue.FeeAmount = EditFeeAmount;
		revenue.GrossAmount = grossAmount;
		revenue.NetAmount = grossAmount - EditFeeAmount;
		revenue.TransactionDate = EditTransactionDate;
		await RevenueAdapter.UpdateAsync(revenue);

		var manualEntries = await RevenueManualAdapter.FindAsync(m => m.Id == EditingId.Value);
		var manual = manualEntries.FirstOrDefault();

		if (manual is not null)
		{
			manual.Category = EditCategory;
			manual.Notes = string.IsNullOrWhiteSpace(EditNotes) ? null : EditNotes;
			manual.Quantity = EditQuantity;
			await RevenueManualAdapter.UpdateAsync(manual);
		}

		EditingId = null;
		ShowEditNotes = false;
		await LoadEntriesAsync();
	}

	public void StartEdit(long entryId)
	{
		var entry = Entries.FirstOrDefault(e => e.Id == entryId);
		if (entry is null) return;

		EditingId = entryId;
		EditCategory = entry.Category;
		EditCurrency = entry.Currency;
		EditDescription = entry.Description;
		EditFeeAmount = entry.FeeAmount;
		EditNotes = entry.Notes ?? string.Empty;
		EditQuantity = entry.Quantity;
		EditTransactionDate = entry.TransactionDate;
		EditUnitPrice = entry.UnitPrice;
		ShowEditNotes = !string.IsNullOrWhiteSpace(entry.Notes);
	}

	public void ToggleEditNotes()
	{
		ShowEditNotes = !ShowEditNotes;
	}

	// ── Expense CRUD ──

	public void CancelDeleteExpense()
	{
		DeletingExpenseId = null;
	}

	public void CancelEditExpense()
	{
		ExpenseEditingId = null;
	}

	public async Task ConfirmDeleteExpenseAsync()
	{
		if (DeletingExpenseId is null) return;

		var expenseId = DeletingExpenseId.Value;
		DeletingExpenseId = null;

		var expense = await ExpenseAdapter.GetByIdAsync(expenseId);
		if (expense is null) return;

		await ExpenseAdapter.DeleteAsync(expense);
		await LoadExpensesAsync();
	}

	public void DeleteExpense(long expenseId)
	{
		DeletingExpenseId = expenseId;
	}

	public string GetExpenseSortIndicator(string column) => ExpenseSortColumn == column ? (ExpenseSortDescending ? "▼" : "▲") : "";

	public void GoToExpensePage(int page)
	{
		ExpenseCurrentPage = Math.Clamp(page, 1, TotalExpensePages);
	}

	public async Task HandleExpenseEditKeyDown(KeyboardEventArgs e)
	{
		if (e.Key == "Enter")
			await SaveEditExpenseAsync();
		else if (e.Key == "Escape")
			CancelEditExpense();
	}

	public async Task HandleExpenseQuickEntryKeyDown(KeyboardEventArgs e)
	{
		if (e.Key == "Enter")
			await QuickAddExpenseAsync();
	}

	public async Task QuickAddExpenseAsync()
	{
		if (_userId is null || string.IsNullOrWhiteSpace(NewExpenseDescription) || NewExpenseAmount <= 0)
			return;

		var expense = ToNewExpense();
		await ExpenseAdapter.InsertAsync(expense);

		ResetExpenseQuickEntry();
		await LoadExpensesAsync();

		try { await ExpenseQuickEntryDescriptionRef.FocusAsync(); } catch { /* element may not be rendered yet */ }
	}

	public async Task SaveEditExpenseAsync()
	{
		if (ExpenseEditingId is null || _userId is null) return;

		if (string.IsNullOrWhiteSpace(EditExpenseDescription) || EditExpenseAmount <= 0)
			return;

		var expense = await ExpenseAdapter.GetByIdAsync(ExpenseEditingId.Value);
		if (expense is null) return;

		expense.Amount = EditExpenseAmount;
		expense.Category = EditExpenseCategory;
		expense.Description = EditExpenseDescription;
		expense.ExpenseDate = EditExpenseDate;
		await ExpenseAdapter.UpdateAsync(expense);

		ExpenseEditingId = null;
		await LoadExpensesAsync();
	}

	public void SortExpensesBy(string column)
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
	}

	public void StartEditExpense(long expenseId)
	{
		var expense = Expenses.FirstOrDefault(e => e.Id == expenseId);
		if (expense is null) return;

		ExpenseEditingId = expenseId;
		EditExpenseAmount = expense.Amount;
		EditExpenseCategory = expense.Category;
		EditExpenseDate = expense.ExpenseDate;
		EditExpenseDescription = expense.Description;
	}

	// ── Payout CRUD ──

	public void CancelDeletePayout()
	{
		DeletingPayoutId = null;
	}

	public void CancelEditPayout()
	{
		PayoutEditingId = null;
	}

	public async Task ConfirmDeletePayoutAsync()
	{
		if (DeletingPayoutId is null) return;

		var payoutId = DeletingPayoutId.Value;
		DeletingPayoutId = null;

		var payout = await PayoutAdapter.GetByIdAsync(payoutId);
		if (payout is null) return;

		await PayoutAdapter.DeleteAsync(payout);
		await LoadPayoutsAsync();
	}

	public void DeletePayout(long payoutId)
	{
		DeletingPayoutId = payoutId;
	}

	public string GetPayoutSortIndicator(string column) => PayoutSortColumn == column ? (PayoutSortDescending ? "▼" : "▲") : "";

	public void GoToPayoutPage(int page)
	{
		PayoutCurrentPage = Math.Clamp(page, 1, TotalPayoutPages);
	}

	public async Task HandlePayoutEditKeyDown(KeyboardEventArgs e)
	{
		if (e.Key == "Enter")
			await SaveEditPayoutAsync();
		else if (e.Key == "Escape")
			CancelEditPayout();
	}

	public async Task HandlePayoutQuickEntryKeyDown(KeyboardEventArgs e)
	{
		if (e.Key == "Enter")
			await QuickAddPayoutAsync();
	}

	public async Task QuickAddPayoutAsync()
	{
		if (_userId is null || NewPayoutAmount <= 0)
			return;

		var payout = ToNewPayout();
		await PayoutAdapter.InsertAsync(payout);

		ResetPayoutQuickEntry();
		await LoadPayoutsAsync();

		try { await PayoutQuickEntryAmountRef.FocusAsync(); } catch { /* element may not be rendered yet */ }
	}

	public async Task SaveEditPayoutAsync()
	{
		if (PayoutEditingId is null || _userId is null) return;

		if (EditPayoutAmount <= 0)
			return;

		var payout = await PayoutAdapter.GetByIdAsync(PayoutEditingId.Value);
		if (payout is null) return;

		payout.Amount = EditPayoutAmount;
		payout.ExpectedArrivalDate = EditPayoutExpectedArrival;
		payout.PayoutDate = EditPayoutDate;
		payout.Status = EditPayoutStatus;
		await PayoutAdapter.UpdateAsync(payout);

		PayoutEditingId = null;
		await LoadPayoutsAsync();
	}

	public void SortPayoutsBy(string column)
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
	}

	public void StartEditPayout(long payoutId)
	{
		var payout = Payouts.FirstOrDefault(p => p.Id == payoutId);
		if (payout is null) return;

		PayoutEditingId = payoutId;
		EditPayoutAmount = payout.Amount;
		EditPayoutDate = payout.PayoutDate;
		EditPayoutExpectedArrival = payout.ExpectedArrivalDate;
		EditPayoutStatus = payout.Status;
	}

	// ── Shops ──

	public void CancelAddShop()
	{
		IsAddingShop = false;
		NewShopName = string.Empty;
	}

	public void CancelEditShop()
	{
		EditingShopId = null;
		EditShopName = string.Empty;
	}

	public void CloseShopDropdown()
	{
		IsShopDropdownOpen = false;
	}

	public async Task ConfirmAddShopAsync()
	{
		if (_userId is null) return;

		var name = (NewShopName ?? string.Empty).Trim();
		if (name.Length == 0) return;

		var shop = await CreateManualShopCommand.ExecuteAsync(_userId.Value, name);

		Shops = Shops.Append(shop).OrderBy(s => s.ShopName, StringComparer.OrdinalIgnoreCase).ToList();
		ActiveShopConnectionId = shop.Id;

		IsAddingShop = false;
		NewShopName = string.Empty;

		await LoadEntriesAsync();
		await LoadExpensesAsync();
		await LoadPayoutsAsync();
	}

	public async Task ConfirmEditShopAsync()
	{
		if (_userId is null || EditingShopId is null) return;

		var name = (EditShopName ?? string.Empty).Trim();
		if (name.Length == 0) return;

		var updated = await RenameManualShopCommand.ExecuteAsync(_userId.Value, EditingShopId.Value, name);
		if (updated is null) return;

		var local = Shops.FirstOrDefault(s => s.Id == updated.Id);
		if (local is not null)
			local.ShopName = updated.ShopName;

		Shops = Shops.OrderBy(s => s.ShopName, StringComparer.OrdinalIgnoreCase).ToList();

		EditingShopId = null;
		EditShopName = string.Empty;
	}

	public async Task HandleAddShopKeyDownAsync(KeyboardEventArgs e)
	{
		if (e.Key == "Enter")
			await ConfirmAddShopAsync();
		else if (e.Key == "Escape")
			CancelAddShop();
	}

	public async Task HandleEditShopKeyDownAsync(KeyboardEventArgs e)
	{
		if (e.Key == "Enter")
			await ConfirmEditShopAsync();
		else if (e.Key == "Escape")
			CancelEditShop();
	}

	public async Task SelectShopAsync(long shopId)
	{
		if (ActiveShopConnectionId == shopId)
		{
			IsShopDropdownOpen = false;
			return;
		}

		ActiveShopConnectionId = shopId;
		IsShopDropdownOpen = false;

		await LoadEntriesAsync();
		await LoadExpensesAsync();
		await LoadPayoutsAsync();
	}

	public async Task StartAddShopAsync()
	{
		IsShopDropdownOpen = false;
		IsAddingShop = true;
		NewShopName = string.Empty;

		await Task.Yield();
		try { await NewShopNameRef.FocusAsync(); } catch { /* element may not be rendered yet */ }
	}

	public Task StartEditActiveShopAsync()
	{
		return ActiveShopConnectionId.HasValue ? StartEditShopAsync(ActiveShopConnectionId.Value) : Task.CompletedTask;
	}

	public async Task StartEditShopAsync(long shopId)
	{
		var shop = Shops.FirstOrDefault(s => s.Id == shopId);
		if (shop is null) return;

		IsShopDropdownOpen = false;
		EditingShopId = shopId;
		EditShopName = shop.ShopName;

		await Task.Yield();
		try { await EditShopNameRef.FocusAsync(); } catch { /* element may not be rendered yet */ }
	}

	public void ToggleShopDropdown()
	{
		if (IsAddingShop) return;
		IsShopDropdownOpen = !IsShopDropdownOpen;
	}

	// ── Shared ──

	public void SelectTab(string tab)
	{
		ActiveTab = tab;
	}

	public async Task SetDensity(string density)
	{
		Density = density;
		await SaveGridPreferencesAsync();
	}

	public async Task SortBy(string column)
	{
		if (SortColumn == column)
		{
			SortDescending = !SortDescending;
		}
		else
		{
			SortColumn = column;
			SortDescending = column == "TransactionDate";
		}

		CurrentPage = 1;
		await SaveGridPreferencesAsync();
	}

	// ── Private ──

	private IEnumerable<ManualEntryItem> GetSortedEntries()
	{
		IEnumerable<ManualEntryItem> sorted = SortColumn switch
		{
			"Description" => SortDescending ? Entries.OrderByDescending(e => e.Description, StringComparer.OrdinalIgnoreCase) : Entries.OrderBy(e => e.Description, StringComparer.OrdinalIgnoreCase),
			"Category" => SortDescending ? Entries.OrderByDescending(e => e.Category) : Entries.OrderBy(e => e.Category),
			"Quantity" => SortDescending ? Entries.OrderByDescending(e => e.Quantity) : Entries.OrderBy(e => e.Quantity),
			"UnitPrice" => SortDescending ? Entries.OrderByDescending(e => e.UnitPrice) : Entries.OrderBy(e => e.UnitPrice),
			"FeeAmount" => SortDescending ? Entries.OrderByDescending(e => e.FeeAmount) : Entries.OrderBy(e => e.FeeAmount),
			"NetAmount" => SortDescending ? Entries.OrderByDescending(e => e.NetAmount) : Entries.OrderBy(e => e.NetAmount),
			_ => SortDescending ? Entries.OrderByDescending(e => e.TransactionDate) : Entries.OrderBy(e => e.TransactionDate),
		};

		return sorted;
	}

	private IEnumerable<ExpenseItem> GetSortedExpenses()
	{
		return ExpenseSortColumn switch
		{
			"Description" => ExpenseSortDescending ? Expenses.OrderByDescending(e => e.Description, StringComparer.OrdinalIgnoreCase) : Expenses.OrderBy(e => e.Description, StringComparer.OrdinalIgnoreCase),
			"Category" => ExpenseSortDescending ? Expenses.OrderByDescending(e => e.Category) : Expenses.OrderBy(e => e.Category),
			"Amount" => ExpenseSortDescending ? Expenses.OrderByDescending(e => e.Amount) : Expenses.OrderBy(e => e.Amount),
			_ => ExpenseSortDescending ? Expenses.OrderByDescending(e => e.ExpenseDate) : Expenses.OrderBy(e => e.ExpenseDate),
		};
	}

	private IEnumerable<PayoutItem> GetSortedPayouts()
	{
		return PayoutSortColumn switch
		{
			"Amount" => PayoutSortDescending ? Payouts.OrderByDescending(p => p.Amount) : Payouts.OrderBy(p => p.Amount),
			"Status" => PayoutSortDescending ? Payouts.OrderByDescending(p => p.Status) : Payouts.OrderBy(p => p.Status),
			_ => PayoutSortDescending ? Payouts.OrderByDescending(p => p.PayoutDate) : Payouts.OrderBy(p => p.PayoutDate),
		};
	}

	private async Task LoadEntriesAsync()
	{
		if (ActiveShopConnectionId is null)
		{
			Entries = [];
			CurrentPage = 1;
			return;
		}

		var shopId = ActiveShopConnectionId.Value;
		var revenues = await RevenueAdapter.FindAsync(r => r.UserId == _userId!.Value && r.Platform == "Manual" && r.ShopConnectionId == shopId);
		var manuals = await RevenueManualAdapter.GetAllAsync();

		var manualLookup = manuals.ToDictionary(m => m.Id);

		Entries = revenues
			.Select(r => ToEntryItem(r, manualLookup.GetValueOrDefault(r.Id)))
			.ToList();

		CurrentPage = 1;
	}

	private async Task LoadExpensesAsync()
	{
		if (ActiveShopConnectionId is null)
		{
			Expenses = [];
			return;
		}

		var shopId = ActiveShopConnectionId.Value;
		var expenses = await ExpenseAdapter.FindAsync(e => e.UserId == _userId!.Value && e.Platform == "Manual" && e.ShopConnectionId == shopId);
		Expenses = expenses.Select(ToExpenseItem).ToList();
	}

	private async Task LoadPayoutsAsync()
	{
		if (ActiveShopConnectionId is null)
		{
			Payouts = [];
			return;
		}

		var shopId = ActiveShopConnectionId.Value;
		var payouts = await PayoutAdapter.FindAsync(p => p.UserId == _userId!.Value && p.Platform == "Manual" && p.ShopConnectionId == shopId);
		Payouts = payouts.Select(ToPayoutItem).ToList();
	}

	private async Task LoadShopsAsync()
	{
		if (_userId is null) return;

		var manualPlatform = (await PlatformConnectionAdapter.FindAsync(p => p.UserId == _userId.Value && p.Platform == "Manual")).FirstOrDefault();

		if (manualPlatform is null)
		{
			var shop = await CreateManualShopCommand.ExecuteAsync(_userId.Value, "My Manual Shop");
			Shops = [shop];
			ActiveShopConnectionId = shop.Id;
			return;
		}

		var platformId = manualPlatform.Id;
		var shops = await ShopConnectionAdapter.FindAsync(s => s.PlatformConnectionId == platformId);
		Shops = shops.OrderBy(s => s.ShopName, StringComparer.OrdinalIgnoreCase).ToList();

		if (Shops.Count == 0)
		{
			var shop = await CreateManualShopCommand.ExecuteAsync(_userId.Value, "My Manual Shop");
			Shops = [shop];
			ActiveShopConnectionId = shop.Id;
			return;
		}

		ActiveShopConnectionId ??= Shops[0].Id;
	}

	private void ResetExpenseQuickEntry()
	{
		NewExpenseAmount = 0;
		NewExpenseCategory = "Other";
		NewExpenseDate = DateTime.Today;
		NewExpenseDescription = string.Empty;
	}

	private void ResetPayoutQuickEntry()
	{
		NewPayoutAmount = 0;
		NewPayoutDate = DateTime.Today;
		NewPayoutStatus = "Pending";
	}

	private void ResetQuickEntry()
	{
		NewCategory = "Sale";
		NewCurrency = "USD";
		NewDescription = string.Empty;
		NewFeeAmount = 0;
		NewNotes = string.Empty;
		NewQuantity = 1;
		NewTransactionDate = DateTime.Now;
		NewUnitPrice = 0;
	}

	private async Task SaveGridPreferencesAsync()
	{
		if (_userId is null) return;

		var user = await UserAdapter.GetByIdAsync(_userId.Value);
		if (user is null) return;

		var preferences = PreferencesSerializer.Deserialize(user.UserPreferences);

		preferences.GridPreferences["manualEntry.entryGrid"] = new MODELS.GridPreference
		{
			Density = Density,
			PageSize = PageSize,
			SortColumn = SortColumn,
			SortDescending = SortDescending,
		};

		user.UserPreferences = PreferencesSerializer.Serialize(preferences);
		await UserAdapter.UpdateAsync(user);
	}

	private ManualEntryItem ToEntryItem(MODELS.Revenue revenue, MODELS.RevenueManual? manual)
	{
		return new ManualEntryItem
		{
			Category = manual?.Category ?? "Other",
			Currency = revenue.Currency,
			Description = revenue.Description,
			FeeAmount = revenue.FeeAmount,
			GrossAmount = revenue.GrossAmount,
			Id = revenue.Id,
			NetAmount = revenue.NetAmount,
			Notes = manual?.Notes,
			Quantity = manual?.Quantity ?? 1,
			TransactionDate = revenue.TransactionDate,
		};
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

	private MODELS.Expense ToNewExpense()
	{
		return new MODELS.Expense
		{
			Amount = NewExpenseAmount,
			Category = NewExpenseCategory,
			Currency = "USD",
			Description = NewExpenseDescription,
			ExpenseDate = NewExpenseDate,
			Platform = "Manual",
			ShopConnectionId = ActiveShopConnectionId,
			UserId = _userId!.Value,
		};
	}

	private MODELS.Payout ToNewPayout()
	{
		return new MODELS.Payout
		{
			Amount = NewPayoutAmount,
			Currency = "USD",
			PayoutDate = NewPayoutDate,
			Platform = "Manual",
			PlatformPayoutId = $"manual_{Guid.NewGuid():N}",
			ShopConnectionId = ActiveShopConnectionId,
			Status = NewPayoutStatus,
			UserId = _userId!.Value,
		};
	}

	private MODELS.Revenue ToNewRevenue(decimal grossAmount)
	{
		return new MODELS.Revenue
		{
			Currency = NewCurrency,
			Description = NewDescription,
			FeeAmount = NewFeeAmount,
			GrossAmount = grossAmount,
			NetAmount = grossAmount - NewFeeAmount,
			Platform = "Manual",
			ShopConnectionId = ActiveShopConnectionId,
			TransactionDate = NewTransactionDate,
			UserId = _userId!.Value,
		};
	}

	private MODELS.RevenueManual ToNewRevenueManual(long revenueId)
	{
		return new MODELS.RevenueManual
		{
			Category = NewCategory,
			Id = revenueId,
			Notes = string.IsNullOrWhiteSpace(NewNotes) ? null : NewNotes,
			Quantity = NewQuantity,
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
