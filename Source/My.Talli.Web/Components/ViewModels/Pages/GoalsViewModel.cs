namespace My.Talli.Web.Components.ViewModels.Pages;

using Domain.Data.Interfaces;
using Domain.Framework;
using Domain.Repositories;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Models;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>View Model</summary>
public class GoalsViewModel : ComponentBase
{
	#region <Variables>

	private List<MODELS.Revenue> _cachedRevenues = [];
	private long? _userId;

	[CascadingParameter]
	private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

	[Inject]
	private ICurrentUserService CurrentUserService { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.Goal, ENTITIES.Goal> GoalAdapter { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.GoalType, ENTITIES.GoalType> GoalTypeAdapter { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.Revenue, ENTITIES.Revenue> RevenueAdapter { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.Subscription, ENTITIES.Subscription> SubscriptionAdapter { get; set; } = default!;

	#endregion

	#region <Properties>

	public int ActiveGoalCount => Goals.Count;

	public List<string> AvailablePlatforms { get; private set; } = [];

	public long? DeletingGoalId { get; private set; }

	public string DeletingGoalDescription => Goals.FirstOrDefault(g => g.Id == DeletingGoalId)?.Name ?? "";

	public long? EditingGoalId { get; private set; }

	public DateTime? FormEndDate { get; set; }

	public long FormGoalTypeId { get; set; } = 1;

	public string? FormPlatform { get; set; }

	public DateTime FormStartDate { get; set; } = DateTime.Today;

	public decimal FormTargetAmount { get; set; }

	public decimal FormMatchingRevenue
	{
		get
		{
			var matching = _cachedRevenues.Where(r => r.TransactionDate >= FormStartDate);

			if (FormEndDate.HasValue)
				matching = matching.Where(r => r.TransactionDate <= FormEndDate.Value);

			if (!string.IsNullOrEmpty(FormPlatform))
				matching = matching.Where(r => r.Platform == FormPlatform);

			return matching.Sum(r => r.NetAmount);
		}
	}

	public bool FormHasMatchingRevenue => FormMatchingRevenue > 0;

	public List<GoalItem> Goals { get; private set; } = [];

	public List<MODELS.GoalType> GoalTypes { get; private set; } = [];

	public bool HasModuleAccess { get; private set; }

	public bool IsLoading { get; private set; } = true;

	public bool IsSampleData { get; private set; }

	public int OnTrackCount => Goals.Count(g => g.Status is "On track" or "Ahead");

	public bool ShowAddForm { get; private set; }

	public string TotalEarned => Goals.Any() ? Goals.Sum(g => g.Earned).ToString("C0") : "$0";

	#endregion

	#region <Events>

	protected override async Task OnInitializedAsync()
	{
		var authState = await AuthenticationStateTask;
		var principal = authState.User;

		if (principal.Identity?.IsAuthenticated != true)
		{
			IsSampleData = true;
			Goals = GoalsDataset.GetGoals();
			IsLoading = false;
			return;
		}

		var userIdClaim = principal.FindFirst("UserId")?.Value;
		if (userIdClaim is null || !long.TryParse(userIdClaim, out var userId))
		{
			IsSampleData = true;
			Goals = GoalsDataset.GetGoals();
			IsLoading = false;
			return;
		}

		_userId = userId;
		CurrentUserService.Set(userId, string.Empty);

		// Check for data sources: modules (ProductId >= 3) or platforms (not yet implemented)
		var moduleSubscriptions = await SubscriptionAdapter.FindAsync(s =>
			s.UserId == userId &&
			s.ProductId >= 3 &&
			(s.Status == SubscriptionStatuses.Active || s.Status == SubscriptionStatuses.Cancelling));

		var hasModules = moduleSubscriptions.Any();
		var hasPlatforms = false; // Stub — no platform integrations yet

		HasModuleAccess = hasModules || hasPlatforms;
		IsSampleData = !HasModuleAccess;

		// Load goal type lookup
		var goalTypes = await GoalTypeAdapter.GetAllAsync();
		GoalTypes = goalTypes.OrderBy(gt => gt.Id).ToList();

		if (HasModuleAccess)
		{
			// Load and cache revenue data
			var allRevenues = await RevenueAdapter.FindAsync(r => r.UserId == userId);
			_cachedRevenues = allRevenues.ToList();
			AvailablePlatforms = _cachedRevenues
				.Select(r => r.Platform)
				.Where(p => !string.IsNullOrEmpty(p))
				.Distinct()
				.OrderBy(p => p)
				.ToList();

			await LoadGoalsAsync();
		}
		else
		{
			Goals = GoalsDataset.GetGoals();
		}

		IsLoading = false;
	}

	#endregion

	#region <Methods>

	// ── Form ──

	public void CancelForm()
	{
		EditingGoalId = null;
		ShowAddForm = false;
	}

	public void OpenAddForm()
	{
		EditingGoalId = null;
		FormEndDate = null;
		FormGoalTypeId = 1;
		FormPlatform = null;
		FormStartDate = DateTime.Today;
		FormTargetAmount = 0;
		ShowAddForm = true;
	}

	public async Task SaveAsync()
	{
		if (_userId is null || FormTargetAmount <= 0) return;

		if (EditingGoalId.HasValue)
		{
			// Update existing
			var goal = await GoalAdapter.GetByIdAsync(EditingGoalId.Value);
			if (goal is null) return;

			goal.EndDate = FormEndDate;
			goal.GoalTypeId = FormGoalTypeId;
			goal.Platform = string.IsNullOrEmpty(FormPlatform) ? null : FormPlatform;
			goal.StartDate = FormStartDate;
			goal.TargetAmount = FormTargetAmount;
			await GoalAdapter.UpdateAsync(goal);

			EditingGoalId = null;
		}
		else
		{
			// Insert new
			var goal = new MODELS.Goal
			{
				EndDate = FormEndDate,
				GoalTypeId = FormGoalTypeId,
				Platform = string.IsNullOrEmpty(FormPlatform) ? null : FormPlatform,
				StartDate = FormStartDate,
				Status = "Active",
				TargetAmount = FormTargetAmount,
				UserId = _userId.Value,
			};

			await GoalAdapter.InsertAsync(goal);
			ShowAddForm = false;
		}

		await LoadGoalsAsync();
	}

	public void StartEdit(long goalId)
	{
		ShowAddForm = false;

		var goal = Goals.FirstOrDefault(g => g.Id == goalId);
		if (goal is null) return;

		EditingGoalId = goalId;
		FormEndDate = goal.EndDate;
		FormGoalTypeId = goal.GoalTypeId;
		FormPlatform = goal.Platform;
		FormStartDate = goal.StartDate;
		FormTargetAmount = goal.Target;
	}

	// ── Delete ──

	public void CancelDelete()
	{
		DeletingGoalId = null;
	}

	public async Task ConfirmDeleteAsync()
	{
		if (DeletingGoalId is null) return;

		var goalId = DeletingGoalId.Value;
		DeletingGoalId = null;

		var goal = await GoalAdapter.GetByIdAsync(goalId);
		if (goal is null) return;

		await GoalAdapter.DeleteAsync(goal);
		await LoadGoalsAsync();
	}

	public void DeleteGoal(long goalId)
	{
		DeletingGoalId = goalId;
	}

	// ── Private ──

	private string GenerateGoalLabel(string goalTypeName)
	{
		if (goalTypeName.Contains("Monthly", StringComparison.OrdinalIgnoreCase))
			return "Monthly Target";

		if (goalTypeName.Contains("Yearly", StringComparison.OrdinalIgnoreCase))
			return "Yearly Target";

		if (goalTypeName.Contains("Platform", StringComparison.OrdinalIgnoreCase))
			return "Platform Goal";

		return "Growth Target";
	}

	private string GenerateGoalName(MODELS.Goal goal, string goalTypeName)
	{
		if (!string.IsNullOrEmpty(goal.Platform))
			return $"{goal.Platform} Revenue";

		if (goalTypeName.Contains("Monthly", StringComparison.OrdinalIgnoreCase))
			return $"{goal.StartDate:MMMM} Revenue";

		if (goalTypeName.Contains("Yearly", StringComparison.OrdinalIgnoreCase))
			return $"{goal.StartDate.Year} Revenue";

		return goalTypeName;
	}

	private async Task LoadGoalsAsync()
	{
		var goals = await GoalAdapter.FindAsync(g => g.UserId == _userId!.Value);

		// Refresh cached revenues
		var allRevenues = await RevenueAdapter.FindAsync(r => r.UserId == _userId!.Value);
		_cachedRevenues = allRevenues.ToList();

		var goalTypeLookup = GoalTypes.ToDictionary(gt => gt.Id, gt => gt.Name);
		var today = DateTime.Today;

		Goals = goals.Select(goal =>
		{
			// Compute earned from revenue
			var matchingRevenues = _cachedRevenues
				.Where(r => r.TransactionDate >= goal.StartDate);

			if (goal.EndDate.HasValue)
				matchingRevenues = matchingRevenues.Where(r => r.TransactionDate <= goal.EndDate.Value);

			if (!string.IsNullOrEmpty(goal.Platform))
				matchingRevenues = matchingRevenues.Where(r => r.Platform == goal.Platform);

			var earned = matchingRevenues.Sum(r => r.NetAmount);

			// Compute days remaining
			var effectiveEnd = goal.EndDate ?? new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
			var daysRemaining = Math.Max(0, (effectiveEnd - today).Days);

			// Compute status via pace algorithm
			var daysElapsed = Math.Max(1, (today - goal.StartDate).Days + 1);
			var totalDays = Math.Max(1, (effectiveEnd - goal.StartDate).Days + 1);
			var projectedAmount = earned / daysElapsed * totalDays;

			string status;
			if (projectedAmount >= goal.TargetAmount * 1.1m)
				status = "Ahead";
			else if (projectedAmount >= goal.TargetAmount)
				status = "On track";
			else
				status = "Behind";

			var goalTypeName = goalTypeLookup.GetValueOrDefault(goal.GoalTypeId, "Unknown");

			return new GoalItem
			{
				DaysRemaining = daysRemaining,
				Earned = earned,
				EndDate = goal.EndDate,
				GoalTypeId = goal.GoalTypeId,
				Id = goal.Id,
				Label = GenerateGoalLabel(goalTypeName),
				Name = GenerateGoalName(goal, goalTypeName),
				Platform = goal.Platform,
				StartDate = goal.StartDate,
				Status = status,
				Target = goal.TargetAmount,
			};
		}).ToList();
	}

	#endregion
}
