namespace My.Talli.Web.ViewModels.Pages;

using Domain.Framework;
using Domain.Repositories;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Models;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>View Model</summary>
public class SuggestionBoxViewModel : ComponentBase
{
	#region <Variables>

	private long? _userId;

	[CascadingParameter]
	private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.Suggestion, ENTITIES.Suggestion> SuggestionAdapter { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.SuggestionVote, ENTITIES.SuggestionVote> VoteAdapter { get; set; } = default!;


	#endregion

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

	public string NewCategory { get; set; } = "Feature";

	public string NewDescription { get; set; } = string.Empty;

	public string NewTitle { get; set; } = string.Empty;

	public bool ShowSubmitModal { get; private set; }

	public int TotalIdeas => Suggestions.Count;

	public int TotalVotes => Suggestions.Sum(s => s.Votes);

	public int YourSuggestions => Suggestions.Count(s => s.IsOwn);


	#endregion

	#region <Events>

	protected override async Task OnInitializedAsync()
	{
		var authState = await AuthenticationStateTask;
		var principal = authState.User;

		if (principal.Identity?.IsAuthenticated != true)
			return;

		var userIdClaim = principal.FindFirst("UserId")?.Value;
		if (userIdClaim is null || !long.TryParse(userIdClaim, out var userId))
			return;

		_userId = userId;

		await LoadSuggestionsAsync();
	}


	#endregion

	#region <Methods>

	public void CloseModal()
	{
		ShowSubmitModal = false;
	}

	public void OpenModal()
	{
		NewTitle = string.Empty;
		NewDescription = string.Empty;
		NewCategory = "Feature";
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

	public async Task SubmitSuggestionAsync()
	{
		if (_userId is null || string.IsNullOrWhiteSpace(NewTitle))
			return;

		await SuggestionAdapter.InsertAsync(ToNewSuggestion());

		ShowSubmitModal = false;
		await LoadSuggestionsAsync();
	}

	public async Task ToggleVoteAsync(SuggestionItem suggestion)
	{
		if (_userId is null)
			return;

		if (suggestion.HasVoted)
		{
			var votes = await VoteAdapter.FindAsync(v => v.SuggestionId == suggestion.Id && v.UserId == _userId.Value);
			var vote = votes.FirstOrDefault();

			if (vote is not null)
				await VoteAdapter.DeleteAsync(vote);
		}
		else
		{
			var vote = new MODELS.SuggestionVote { SuggestionId = suggestion.Id, UserId = _userId.Value, };
			await VoteAdapter.InsertAsync(vote);
		}

		await LoadSuggestionsAsync();
	}

	private void ApplyFilters()
	{
		var filtered = ActiveCategory == "All"
			? Suggestions
			: Suggestions.Where(s => s.Category == ActiveCategory);

		FilteredSuggestions = ActiveSort switch
		{
			"New" => filtered.OrderByDescending(s => s.CreatedOn).ToList(),
			_ => filtered.OrderByDescending(s => s.Votes).ToList()
		};
	}

	private async Task LoadSuggestionsAsync()
	{
		var allSuggestions = await SuggestionAdapter.GetAllAsync();
		var allVotes = await VoteAdapter.GetAllAsync();

		Suggestions = allSuggestions.Select(s => ToSuggestionItem(s, allVotes)).ToList();

		ApplyFilters();
	}

	private MODELS.Suggestion ToNewSuggestion()
	{
		return new MODELS.Suggestion
		{
			Category = NewCategory,
			Description = NewDescription,
			Status = SuggestionStatuses.Submitted,
			Title = NewTitle,
			UserId = _userId!.Value,
		};
	}

	private SuggestionItem ToSuggestionItem(MODELS.Suggestion s, IEnumerable<MODELS.SuggestionVote> allVotes)
	{
		var votesForSuggestion = allVotes.Where(v => v.SuggestionId == s.Id).ToList();

		return new SuggestionItem {
			Author = s.UserId == _userId ? "You" : $"User #{s.UserId}",
			Category = s.Category,
			CreatedOn = s.CreatedOn,
			Description = s.Description,
			HasVoted = votesForSuggestion.Any(v => v.UserId == _userId),
			Id = s.Id,
			IsOwn = s.UserId == _userId,
			Title = s.Title,
			Votes = votesForSuggestion.Count,
		};
	}

	private List<SuggestionItem> Suggestions { get; set; } = [];

	#endregion
}
