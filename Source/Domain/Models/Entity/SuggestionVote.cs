namespace My.Talli.Domain.Models;

/// <summary>Model</summary>
public class SuggestionVote : DefaultModel
{
	#region <Properties>

	public long SuggestionId { get; set; }

	public long UserId { get; set; }


	#endregion
}
