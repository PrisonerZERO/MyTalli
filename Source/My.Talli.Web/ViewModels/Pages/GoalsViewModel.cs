namespace My.Talli.Web.ViewModels.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Models;

/// <summary>View Model</summary>
public class GoalsViewModel : ComponentBase
{
	#region <Variables>

	[CascadingParameter]
	private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;


	#endregion

	#region <Properties>

	public int ActiveGoalCount => Goals.Count;

	public List<GoalItem> Goals { get; private set; } = [];

	public bool IsLoading { get; private set; } = true;

	public int OnTrackCount => Goals.Count(g => g.Status is "On track" or "Ahead");

	public string TotalEarned => Goals.Max(g => g.Earned).ToString("C0");


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

		// Sample data — will be replaced with real goal queries
		Goals = GetSampleGoals();
		IsLoading = false;
	}


	#endregion

	#region <Methods>

	private static List<GoalItem> GetSampleGoals()
	{
		return
		[
			new GoalItem
			{
				DaysRemaining = 12,
				Earned = 1847m,
				Label = "Monthly Target",
				Name = "March Revenue",
				Status = "On track",
				Target = 2700m,
			},
			new GoalItem
			{
				DaysRemaining = 280,
				Earned = 5547m,
				Label = "Yearly Target",
				Name = "2026 Revenue",
				Status = "Ahead",
				Target = 25000m,
			},
			new GoalItem
			{
				DaysRemaining = 12,
				Earned = 982m,
				Label = "Platform Goal",
				Name = "Stripe Revenue",
				Status = "Behind",
				Target = 2000m,
			},
		];
	}

	#endregion
}
