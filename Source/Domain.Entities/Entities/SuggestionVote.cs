namespace My.Talli.Domain.Entities;

/// <summary>Entity</summary>
public class SuggestionVote : DefaultEntity
{
	#region <Properties>

	public Suggestion Suggestion { get; set; } = null!;

	public long SuggestionId { get; set; }

	public User User { get; set; } = null!;

	public long UserId { get; set; }


	#endregion
}
