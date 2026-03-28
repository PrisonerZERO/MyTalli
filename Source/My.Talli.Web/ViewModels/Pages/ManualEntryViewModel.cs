namespace My.Talli.Web.ViewModels.Pages;

using Domain.Components.JsonSerializers;
using Domain.Data.Interfaces;
using Domain.Framework;
using Domain.Repositories;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
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
	private ICurrentUserService CurrentUserService { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.Revenue, ENTITIES.Revenue> RevenueAdapter { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.RevenueManual, ENTITIES.RevenueManual> RevenueManualAdapter { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.Subscription, ENTITIES.Subscription> SubscriptionAdapter { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.User, ENTITIES.User> UserAdapter { get; set; } = default!;

	[Inject]
	private UserPreferencesJsonSerializer PreferencesSerializer { get; set; } = default!;

	#endregion

	#region <Properties>

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

	public string Density { get; private set; } = "comfortable";

	public long? DeletingId { get; private set; }

	public long? EditingId { get; private set; }

	public List<ManualEntryItem> Entries { get; private set; } = [];

	public bool HasModuleAccess { get; private set; }

	public bool IsSampleData { get; private set; }

	public string ModalTitle => EditingId.HasValue ? "Edit Entry" : "New Entry";

	public string NewCategory { get; set; } = "Sale";

	public string NewCurrency { get; set; } = "USD";

	public string NewDescription { get; set; } = string.Empty;

	public decimal NewFeeAmount { get; set; }

	public string NewNotes { get; set; } = string.Empty;

	public int NewQuantity { get; set; } = 1;

	public DateTime NewTransactionDate { get; set; } = DateTime.Today;

	public decimal NewUnitPrice { get; set; }

	public List<ManualEntryItem> PagedEntries => GetSortedEntries()
		.Skip((CurrentPage - 1) * PageSize)
		.Take(PageSize)
		.ToList();

	public int PageSize { get; set; } = 10;

	public int[] PageSizeOptions { get; } = [10, 25, 50];

	public bool ShowEntryModal { get; private set; }

	public string SortColumn { get; private set; } = "TransactionDate";

	public bool SortDescending { get; private set; } = true;

	public int TotalEntries => Entries.Count;

	public decimal TotalGross => Entries.Sum(e => e.GrossAmount);

	public decimal TotalNet => Entries.Sum(e => e.NetAmount);

	public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)Entries.Count / PageSize));

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
			}
		}

		if (HasModuleAccess)
		{
			await LoadEntriesAsync();
		}
		else
		{
			IsSampleData = true;
			Entries = ManualEntrySampleData.GetEntries();
		}
	}

	#endregion

	#region <Methods>

	public async Task ChangePageSize()
	{
		CurrentPage = 1;
		await SaveGridPreferencesAsync();
	}

	public void CloseModal()
	{
		ShowEntryModal = false;
		EditingId = null;
	}

	public void CancelDelete()
	{
		DeletingId = null;
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

	public void DeleteEntryAsync(long revenueId)
	{
		DeletingId = revenueId;
	}

	public string DeletingDescription => Entries.FirstOrDefault(e => e.Id == DeletingId)?.Description ?? "";

	public string DensityCss => $"density-{Density}";

	public async Task EditEntryAsync(long revenueId)
	{
		var revenue = await RevenueAdapter.GetByIdAsync(revenueId);
		if (revenue is null) return;

		var manualEntries = await RevenueManualAdapter.FindAsync(m => m.Id == revenueId);
		var manual = manualEntries.FirstOrDefault();

		EditingId = revenueId;
		NewDescription = revenue.Description;
		NewFeeAmount = revenue.FeeAmount;
		NewCurrency = revenue.Currency;
		NewTransactionDate = revenue.TransactionDate;
		NewCategory = manual?.Category ?? "Other";
		NewNotes = manual?.Notes ?? string.Empty;
		NewQuantity = manual?.Quantity ?? 1;
		NewUnitPrice = NewQuantity > 0 ? revenue.GrossAmount / NewQuantity : revenue.GrossAmount;
		ShowEntryModal = true;
	}

	public string GetSortIndicator(string column) => SortColumn == column ? (SortDescending ? "▼" : "▲") : "";

	public void GoToPage(int page)
	{
		CurrentPage = Math.Clamp(page, 1, TotalPages);
	}

	public void OpenModal()
	{
		EditingId = null;
		NewCategory = "Sale";
		NewCurrency = "USD";
		NewDescription = string.Empty;
		NewFeeAmount = 0;
		NewNotes = string.Empty;
		NewQuantity = 1;
		NewTransactionDate = DateTime.Today;
		NewUnitPrice = 0;
		ShowEntryModal = true;
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

	public async Task SubmitEntryAsync()
	{
		var grossAmount = NewUnitPrice * NewQuantity;
		if (_userId is null || string.IsNullOrWhiteSpace(NewDescription) || grossAmount <= 0)
			return;

		if (EditingId.HasValue)
		{
			// Update existing
			var revenue = await RevenueAdapter.GetByIdAsync(EditingId.Value);
			if (revenue is null) return;

			revenue.Currency = NewCurrency;
			revenue.Description = NewDescription;
			revenue.FeeAmount = NewFeeAmount;
			revenue.GrossAmount = grossAmount;
			revenue.NetAmount = grossAmount - NewFeeAmount;
			revenue.TransactionDate = NewTransactionDate;
			await RevenueAdapter.UpdateAsync(revenue);

			var manualEntries = await RevenueManualAdapter.FindAsync(m => m.Id == EditingId.Value);
			var manual = manualEntries.FirstOrDefault();

			if (manual is not null)
			{
				manual.Category = NewCategory;
				manual.Notes = string.IsNullOrWhiteSpace(NewNotes) ? null : NewNotes;
				manual.Quantity = NewQuantity;
				await RevenueManualAdapter.UpdateAsync(manual);
			}
		}
		else
		{
			// Insert new
			var revenue = ToNewRevenue(grossAmount);
			revenue = await RevenueAdapter.InsertAsync(revenue);

			var manual = ToNewRevenueManual(revenue.Id);
			await RevenueManualAdapter.InsertAsync(manual);
		}

		ShowEntryModal = false;
		EditingId = null;
		await LoadEntriesAsync();
	}

	private async Task LoadEntriesAsync()
	{
		var revenues = await RevenueAdapter.FindAsync(r => r.UserId == _userId!.Value && r.Platform == "Manual");
		var manuals = await RevenueManualAdapter.GetAllAsync();

		var manualLookup = manuals.ToDictionary(m => m.Id);

		Entries = revenues
			.Select(r => ToEntryItem(r, manualLookup.GetValueOrDefault(r.Id)))
			.ToList();

		CurrentPage = 1;
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

	private IEnumerable<ManualEntryItem> GetSortedEntries()
	{
		IEnumerable<ManualEntryItem> sorted = SortColumn switch
		{
			"Description" => SortDescending ? Entries.OrderByDescending(e => e.Description, StringComparer.OrdinalIgnoreCase) : Entries.OrderBy(e => e.Description, StringComparer.OrdinalIgnoreCase),
			"Category" => SortDescending ? Entries.OrderByDescending(e => e.Category) : Entries.OrderBy(e => e.Category),
			"Quantity" => SortDescending ? Entries.OrderByDescending(e => e.Quantity) : Entries.OrderBy(e => e.Quantity),
			"GrossAmount" => SortDescending ? Entries.OrderByDescending(e => e.GrossAmount) : Entries.OrderBy(e => e.GrossAmount),
			"FeeAmount" => SortDescending ? Entries.OrderByDescending(e => e.FeeAmount) : Entries.OrderBy(e => e.FeeAmount),
			"NetAmount" => SortDescending ? Entries.OrderByDescending(e => e.NetAmount) : Entries.OrderBy(e => e.NetAmount),
			_ => SortDescending ? Entries.OrderByDescending(e => e.TransactionDate) : Entries.OrderBy(e => e.TransactionDate),
		};

		return sorted;
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
			PlatformTransactionId = $"manual_{Guid.NewGuid():N}",
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

	#endregion
}
