namespace My.Talli.Web.ViewModels.Pages;

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

	[CascadingParameter]
	private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

	[Inject]
	private ICurrentUserService CurrentUserService { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.Subscription, ENTITIES.Subscription> SubscriptionAdapter { get; set; } = default!;

	#endregion

	#region <Properties>

	public int ActiveGoalCount => Goals.Count;

	public List<GoalItem> Goals { get; private set; } = [];

	public bool IsLoading { get; private set; } = true;

	public bool IsSampleData { get; private set; }

	public int OnTrackCount => Goals.Count(g => g.Status is "On track" or "Ahead");

	public string TotalEarned => Goals.Any() ? Goals.Max(g => g.Earned).ToString("C0") : "$0";

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

		CurrentUserService.Set(userId, string.Empty);

		// Check for data sources: modules (ProductId >= 3) or platforms (not yet implemented)
		var moduleSubscriptions = await SubscriptionAdapter.FindAsync(s =>
			s.UserId == userId &&
			s.ProductId >= 3 &&
			(s.Status == SubscriptionStatuses.Active || s.Status == SubscriptionStatuses.Cancelling));

		var hasModules = moduleSubscriptions.Any();
		var hasPlatforms = false; // Stub — no platform integrations yet

		IsSampleData = !hasModules && !hasPlatforms;

		// Load goals (sample data for now — real Goal entity not yet built)
		Goals = GoalsDataset.GetGoals();
		IsLoading = false;
	}

	#endregion
}
