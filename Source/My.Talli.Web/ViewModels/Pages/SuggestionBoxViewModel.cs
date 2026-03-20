namespace My.Talli.Web.ViewModels.Pages;

using Microsoft.AspNetCore.Components;

/// <summary>View Model</summary>
public class SuggestionBoxViewModel : ComponentBase
{
    #region <Properties>

    public string ActiveCategory { get; private set; } = "All";

    public string ActiveSort { get; private set; } = "Top";

    public List<string> Categories { get; private set; } =
    [
        "All",
        "Feature",
        "Integration",
        "Export",
        "UI / UX"
    ];

    public List<SuggestionItem> FilteredSuggestions { get; private set; } = [];

    public bool ShowSubmitModal { get; private set; }

    public int TotalIdeas => Suggestions.Count;

    public int TotalVotes => Suggestions.Sum(s => s.Votes);

    public int YourSuggestions => Suggestions.Count(s => s.IsOwn);


    #endregion

    #region <Variables>

    private List<SuggestionItem> Suggestions { get; set; } = [];


    #endregion

    #region <Events>

    protected override void OnInitialized()
    {
        Suggestions = GetMockSuggestions();
        ApplyFilters();
    }


    #endregion

    #region <Methods>

    public void CloseModal()
    {
        ShowSubmitModal = false;
    }

    public void OpenModal()
    {
        ShowSubmitModal = true;
    }

    public void SelectCategory(string category)
    {
        ActiveCategory = category;
        ApplyFilters();
    }

    public void SelectSort(string sort)
    {
        ActiveSort = sort;
        ApplyFilters();
    }

    public void ToggleVote(SuggestionItem suggestion)
    {
        if (suggestion.HasVoted)
        {
            suggestion.HasVoted = false;
            suggestion.Votes--;
        }
        else
        {
            suggestion.HasVoted = true;
            suggestion.Votes++;
        }
    }

    private void ApplyFilters()
    {
        var filtered = ActiveCategory == "All"
            ? Suggestions
            : Suggestions.Where(s => s.Category == ActiveCategory);

        FilteredSuggestions = ActiveSort switch
        {
            "New" => filtered.OrderByDescending(s => s.DaysAgo == 0 ? 0 : s.DaysAgo).ToList(),
            _ => filtered.OrderByDescending(s => s.Votes).ToList()
        };
    }

    private static List<SuggestionItem> GetMockSuggestions()
    {
        return
        [
            new SuggestionItem
            {
                Title = "Dark Mode Support",
                Description = "Would love a dark mode option for late-night revenue tracking. The current light theme can be harsh on the eyes.",
                Category = "Feature",
                Author = "Sarah J.",
                DaysAgo = 3,
                Votes = 24,
                HasVoted = true,
                IsOwn = true
            },
            new SuggestionItem
            {
                Title = "CSV Export with Date Range",
                Description = "Allow users to select a custom date range when exporting CSV data, rather than exporting everything at once.",
                Category = "Export",
                Author = "Alex M.",
                DaysAgo = 5,
                Votes = 18,
                HasVoted = false,
                IsOwn = false
            },
            new SuggestionItem
            {
                Title = "Stripe Webhook Real-Time Sync",
                Description = "Use Stripe webhooks so new transactions appear in the dashboard instantly instead of polling.",
                Category = "Integration",
                Author = "Jordan K.",
                DaysAgo = 7,
                Votes = 15,
                HasVoted = false,
                IsOwn = false
            },
            new SuggestionItem
            {
                Title = "Multi-Currency Support",
                Description = "Support displaying and converting revenue in different currencies for international sellers.",
                Category = "Feature",
                Author = "Priya R.",
                DaysAgo = 7,
                Votes = 12,
                HasVoted = false,
                IsOwn = true
            },
            new SuggestionItem
            {
                Title = "Customizable Dashboard Widgets",
                Description = "Let users drag and rearrange dashboard cards to prioritize what they care about most.",
                Category = "UI / UX",
                Author = "Chris T.",
                DaysAgo = 14,
                Votes = 9,
                HasVoted = false,
                IsOwn = false
            },
            new SuggestionItem
            {
                Title = "Weekly Email Summary",
                Description = "Get a weekly digest email with revenue highlights, top-performing platforms, and goal progress.",
                Category = "Feature",
                Author = "Maya L.",
                DaysAgo = 14,
                Votes = 6,
                HasVoted = false,
                IsOwn = true
            }
        ];
    }


    #endregion
}

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

    public int DaysAgo { get; set; }

    public string Description { get; set; } = "";

    public bool HasVoted { get; set; }

    public bool IsOwn { get; set; }

    public string TimeLabel => DaysAgo switch
    {
        0 => "today",
        1 => "1d",
        < 7 => $"{DaysAgo}d",
        < 14 => "1w",
        < 30 => $"{DaysAgo / 7}w",
        _ => $"{DaysAgo / 30}mo"
    };

    public string Title { get; set; } = "";

    public int Votes { get; set; }


    #endregion
}
