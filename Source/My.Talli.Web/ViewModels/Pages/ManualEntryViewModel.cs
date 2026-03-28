namespace My.Talli.Web.ViewModels.Pages;

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
	private RepositoryAdapterAsync<MODELS.Revenue, ENTITIES.Revenue> RevenueAdapter { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.RevenueManual, ENTITIES.RevenueManual> RevenueManualAdapter { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.Subscription, ENTITIES.Subscription> SubscriptionAdapter { get; set; } = default!;

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

	public long? EditingId { get; private set; }

	public List<ManualEntryItem> Entries { get; private set; } = [];

	public bool HasModuleAccess { get; private set; }

	public string NewCategory { get; set; } = "Sale";

	public string NewCurrency { get; set; } = "USD";

	public string NewDescription { get; set; } = string.Empty;

	public decimal NewFeeAmount { get; set; }

	public decimal NewGrossAmount { get; set; }

	public string NewNotes { get; set; } = string.Empty;

	public DateTime NewTransactionDate { get; set; } = DateTime.Today;

	public bool ShowEntryModal { get; private set; }

	public string ModalTitle => EditingId.HasValue ? "Edit Entry" : "New Entry";

	public int TotalEntries => Entries.Count;

	public decimal TotalGross => Entries.Sum(e => e.GrossAmount);

	public decimal TotalNet => Entries.Sum(e => e.NetAmount);

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

		// Check module access: active subscription for Manual Entry Module (ProductId = 3)
		var subscriptions = await SubscriptionAdapter.FindAsync(s =>
			s.UserId == userId &&
			s.ProductId == 3 &&
			(s.Status == SubscriptionStatuses.Active || s.Status == SubscriptionStatuses.Cancelling));

		HasModuleAccess = subscriptions.Any();

		if (HasModuleAccess)
			await LoadEntriesAsync();
	}

	#endregion

	#region <Methods>

	public void CloseModal()
	{
		ShowEntryModal = false;
		EditingId = null;
	}

	public async Task DeleteEntryAsync(long revenueId)
	{
		var revenue = await RevenueAdapter.GetByIdAsync(revenueId);
		if (revenue is null) return;

		var manualEntries = await RevenueManualAdapter.FindAsync(m => m.Id == revenueId);
		var manual = manualEntries.FirstOrDefault();

		if (manual is not null)
			await RevenueManualAdapter.DeleteAsync(manual);

		await RevenueAdapter.DeleteAsync(revenue);
		await LoadEntriesAsync();
	}

	public async Task EditEntryAsync(long revenueId)
	{
		var revenue = await RevenueAdapter.GetByIdAsync(revenueId);
		if (revenue is null) return;

		var manualEntries = await RevenueManualAdapter.FindAsync(m => m.Id == revenueId);
		var manual = manualEntries.FirstOrDefault();

		EditingId = revenueId;
		NewDescription = revenue.Description;
		NewGrossAmount = revenue.GrossAmount;
		NewFeeAmount = revenue.FeeAmount;
		NewCurrency = revenue.Currency;
		NewTransactionDate = revenue.TransactionDate;
		NewCategory = manual?.Category ?? "Other";
		NewNotes = manual?.Notes ?? string.Empty;
		ShowEntryModal = true;
	}

	public void OpenModal()
	{
		EditingId = null;
		NewDescription = string.Empty;
		NewGrossAmount = 0;
		NewFeeAmount = 0;
		NewCurrency = "USD";
		NewTransactionDate = DateTime.Today;
		NewCategory = "Sale";
		NewNotes = string.Empty;
		ShowEntryModal = true;
	}

	public async Task SubmitEntryAsync()
	{
		if (_userId is null || string.IsNullOrWhiteSpace(NewDescription) || NewGrossAmount <= 0)
			return;

		if (EditingId.HasValue)
		{
			// Update existing
			var revenue = await RevenueAdapter.GetByIdAsync(EditingId.Value);
			if (revenue is null) return;

			revenue.Currency = NewCurrency;
			revenue.Description = NewDescription;
			revenue.FeeAmount = NewFeeAmount;
			revenue.GrossAmount = NewGrossAmount;
			revenue.NetAmount = NewGrossAmount - NewFeeAmount;
			revenue.TransactionDate = NewTransactionDate;
			await RevenueAdapter.UpdateAsync(revenue);

			var manualEntries = await RevenueManualAdapter.FindAsync(m => m.Id == EditingId.Value);
			var manual = manualEntries.FirstOrDefault();

			if (manual is not null)
			{
				manual.Category = NewCategory;
				manual.Notes = string.IsNullOrWhiteSpace(NewNotes) ? null : NewNotes;
				await RevenueManualAdapter.UpdateAsync(manual);
			}
		}
		else
		{
			// Insert new
			var revenue = ToNewRevenue();
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
			.OrderByDescending(r => r.TransactionDate)
			.Select(r => ToEntryItem(r, manualLookup.GetValueOrDefault(r.Id)))
			.ToList();
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
			TransactionDate = revenue.TransactionDate,
		};
	}

	private MODELS.Revenue ToNewRevenue()
	{
		return new MODELS.Revenue
		{
			Currency = NewCurrency,
			Description = NewDescription,
			FeeAmount = NewFeeAmount,
			GrossAmount = NewGrossAmount,
			NetAmount = NewGrossAmount - NewFeeAmount,
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
		};
	}

	#endregion
}
