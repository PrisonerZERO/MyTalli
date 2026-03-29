namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class Goal : DefaultModel
{
	#region <Properties>

	public DateTime CreatedOn { get; set; }

	public DateTime? EndDate { get; set; }

	public long GoalTypeId { get; set; }

	public string? Platform { get; set; }

	public DateTime StartDate { get; set; }

	public string Status { get; set; } = string.Empty;

	public decimal TargetAmount { get; set; }

	public long UserId { get; set; }

	#endregion
}
