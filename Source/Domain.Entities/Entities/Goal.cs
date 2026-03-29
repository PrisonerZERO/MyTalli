namespace My.Talli.Domain.Entities;

/// <summary>Entity</summary>
public class Goal : DefaultEntity
{
	#region <Properties>

	public DateTime? EndDate { get; set; }

	public GoalType GoalType { get; set; } = null!;

	public long GoalTypeId { get; set; }

	public string? Platform { get; set; }

	public DateTime StartDate { get; set; }

	public string Status { get; set; } = string.Empty;

	public decimal TargetAmount { get; set; }

	public User User { get; set; } = null!;

	public long UserId { get; set; }

	#endregion
}
