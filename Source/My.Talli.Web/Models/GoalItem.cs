namespace My.Talli.Web.Models;

/// <summary>Model</summary>
public class GoalItem
{
	#region <Properties>

	public int DaysRemaining { get; set; }

	public decimal Earned { get; set; }

	public DateTime? EndDate { get; set; }

	public long GoalTypeId { get; set; }

	public long Id { get; set; }

	public string Label { get; set; } = "";

	public string Name { get; set; } = "";

	public string? Platform { get; set; }

	public DateTime StartDate { get; set; }

	public int Percentage => Target > 0 ? (int)(Earned / Target * 100) : 0;

	public string Status { get; set; } = "";

	public string StatusCss => Status switch
	{
		"On track" => "on-track",
		"Ahead" => "ahead",
		"Behind" => "behind",
		_ => "on-track"
	};

	public decimal Target { get; set; }


	#endregion
}
