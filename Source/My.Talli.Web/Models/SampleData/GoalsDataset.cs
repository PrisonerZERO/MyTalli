namespace My.Talli.Web.Models;

/// <summary>Sample Data</summary>
public static class GoalsDataset
{
	#region <Methods>

	public static List<GoalItem> GetGoals()
	{
		var now = DateTime.Now;
		var monthStart = new DateTime(now.Year, now.Month, 1);
		var monthEnd = monthStart.AddMonths(1).AddDays(-1);
		var yearStart = new DateTime(now.Year, 1, 1);
		var yearEnd = new DateTime(now.Year, 12, 31);

		return
		[
			new GoalItem
			{
				DaysRemaining = Math.Max(0, (monthEnd - now.Date).Days),
				Earned = 1847m,
				EndDate = monthEnd,
				GoalTypeId = 1,
				Label = "Monthly Target",
				Name = $"{now:MMMM} Revenue",
				StartDate = monthStart,
				Status = "On track",
				Target = 2700m,
			},
			new GoalItem
			{
				DaysRemaining = Math.Max(0, (yearEnd - now.Date).Days),
				Earned = 5547m,
				EndDate = yearEnd,
				GoalTypeId = 2,
				Label = "Yearly Target",
				Name = $"{now.Year} Revenue",
				StartDate = yearStart,
				Status = "Ahead",
				Target = 25000m,
			},
			new GoalItem
			{
				DaysRemaining = Math.Max(0, (monthEnd - now.Date).Days),
				Earned = 982m,
				EndDate = monthEnd,
				GoalTypeId = 3,
				Label = "Platform Goal",
				Name = "Stripe Revenue",
				Platform = "Stripe",
				StartDate = monthStart,
				Status = "Behind",
				Target = 2000m,
			},
		];
	}

	#endregion
}
