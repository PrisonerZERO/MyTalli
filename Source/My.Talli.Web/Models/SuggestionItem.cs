namespace My.Talli.Web.Models;

using My.Talli.Domain.Framework;

/// <summary>Model</summary>
public class SuggestionItem
{
	#region <Properties>

	public string Category { get; set; } = "";

	public string CardCss => Status switch
	{
		SuggestionStatuses.InProgress => "suggest-card-in-progress",
		SuggestionStatuses.Planned => "suggest-card-planned",
		SuggestionStatuses.Completed => "suggest-card-completed",
		SuggestionStatuses.Declined => "suggest-card-declined",
		_ => ""
	};

	public string CategoryCss => Category switch
	{
		"Feature" => "cat-feature",
		"Integration" => "cat-integration",
		"Export" => "cat-export",
		"UI / UX" => "cat-ui",
		_ => "cat-feature"
	};

	public DateTime CreatedOn { get; set; }

	public string DateLabel => CreatedOn.ToString("MMM d, yyyy");

	public string Description { get; set; } = "";

	public bool HasVoted { get; set; }

	public long Id { get; set; }

	public bool IsEditable => IsOwn && Status == SuggestionStatuses.New;

	public bool IsOwn { get; set; }

	public string Status { get; set; } = SuggestionStatuses.New;

	public int StatusSortWeight => Status switch
	{
		SuggestionStatuses.InProgress => 1,
		SuggestionStatuses.Planned => 2,
		SuggestionStatuses.New => 3,
		SuggestionStatuses.UnderReview => 4,
		SuggestionStatuses.Completed => 5,
		SuggestionStatuses.Declined => 6,
		_ => 3
	};

	public string StatusCss => Status switch
	{
		SuggestionStatuses.InProgress => "status-in-progress",
		SuggestionStatuses.Planned => "status-planned",
		SuggestionStatuses.Completed => "status-completed",
		SuggestionStatuses.Declined => "status-declined",
		SuggestionStatuses.UnderReview => "status-under-review",
		_ => ""
	};

	public string StatusLabel => Status switch
	{
		SuggestionStatuses.InProgress => "In Progress",
		SuggestionStatuses.UnderReview => "Under Review",
		_ => Status
	};

	public string Title { get; set; } = "";

	public int Votes { get; set; }

	#endregion
}
