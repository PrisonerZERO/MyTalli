namespace My.Talli.Web.Models;

/// <summary>Sample Data</summary>
public static class GoalsDataset
{
	#region <Methods>

	public static List<GoalItem> GetGoals()
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
