namespace My.Talli.Web.Models;

/// <summary>Model</summary>
public class SuggestionItem
{
	#region <Properties>

	public string Author { get; set; } = "";

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

	public string Description { get; set; } = "";

	public bool HasVoted { get; set; }

	public long Id { get; set; }

	public bool IsOwn { get; set; }

	public string TimeLabel
	{
		get
		{
			var daysAgo = (int)(DateTime.UtcNow - CreatedOn).TotalDays;

			return daysAgo switch
			{
				0 => "today",
				1 => "1d",
				< 7 => $"{daysAgo}d",
				< 14 => "1w",
				< 30 => $"{daysAgo / 7}w",
				_ => $"{daysAgo / 30}mo"
			};
		}
	}

	public string Title { get; set; } = "";

	public int Votes { get; set; }


	#endregion
}
