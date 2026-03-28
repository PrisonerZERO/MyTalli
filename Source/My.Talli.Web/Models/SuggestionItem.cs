namespace My.Talli.Web.Models;

/// <summary>Model</summary>
public class SuggestionItem
{
	#region <Properties>

	public string Category { get; set; } = "";

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

	public bool IsOwn { get; set; }

	public string Title { get; set; } = "";

	public int Votes { get; set; }


	#endregion
}
