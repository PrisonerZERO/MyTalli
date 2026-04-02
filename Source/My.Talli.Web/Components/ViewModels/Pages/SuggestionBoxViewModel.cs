namespace My.Talli.Web.Components.ViewModels.Pages;

using Domain.Data.Interfaces;
using Domain.Framework;
using Domain.Repositories;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Models;
using System.Security.Claims;

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
	private ICurrentUserService CurrentUserService { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.Suggestion, ENTITIES.Suggestion> SuggestionAdapter { get; set; } = default!;

	[Inject]
	private RepositoryAdapterAsync<MODELS.SuggestionVote, ENTITIES.SuggestionVote> VoteAdapter { get; set; } = default!;


	#endregion

	#region <Properties>

	public string ActiveCategory { get; private set; } = "All";

	public string ActiveSort { get; private set; } = "Top";

	public long? EditingId { get; private set; }

	public long? EditingNoteId { get; private set; }

	public List<string> Categories { get; private set; } =
	[
		"All",
		"Feature",
		"Integration",
		"Export",
		"UI / UX"
	];

	public List<SuggestionItem> FilteredSuggestions { get; private set; } = [];

	public bool IsAdmin { get; private set; }

	public string NewCategory { get; set; } = "Feature";

	public string NewDescription { get; set; } = string.Empty;

	public string NewTitle { get; set; } = string.Empty;

	public string NoteText { get; set; } = string.Empty;

	public string ModalTitle => EditingId.HasValue ? "Edit Suggestion" : "New Suggestion";

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
		IsAdmin = principal.IsInRole(Roles.Admin);
		CurrentUserService.Set(userId, string.Empty);

		await LoadSuggestionsAsync();
	}


	#endregion

	#region <Methods>

	public void CloseModal()
	{
		ShowSubmitModal = false;
		EditingId = null;
	}

	public async Task EditSuggestionAsync(long suggestionId)
	{
		var suggestion = await SuggestionAdapter.GetByIdAsync(suggestionId);
		if (suggestion is null || suggestion.Status != SuggestionStatuses.New) return;

		EditingId = suggestionId;
		NewTitle = suggestion.Title;
		NewDescription = suggestion.Description;
		NewCategory = suggestion.Category;
		ShowSubmitModal = true;
	}

	public void OpenModal()
	{
		EditingId = null;
		NewTitle = string.Empty;
		NewDescription = string.Empty;
		NewCategory = "Feature";
		ShowSubmitModal = true;
	}

	public async Task SetStatusAsync(long suggestionId, string status)
	{
		if (!IsAdmin) return;

		var suggestion = await SuggestionAdapter.GetByIdAsync(suggestionId);
		if (suggestion is null || suggestion.Status == status) return;

		suggestion.Status = status;
		await SuggestionAdapter.UpdateAsync(suggestion);
		await LoadSuggestionsAsync();
	}

	public void StartEditNote(SuggestionItem item)
	{
		if (!IsAdmin) return;
		EditingNoteId = item.Id;
		NoteText = item.AdminNote ?? string.Empty;
	}

	public void CancelEditNote()
	{
		EditingNoteId = null;
		NoteText = string.Empty;
	}

	public async Task SaveNoteAsync()
	{
		if (!IsAdmin || EditingNoteId is null) return;

		var suggestion = await SuggestionAdapter.GetByIdAsync(EditingNoteId.Value);
		if (suggestion is null) return;

		suggestion.AdminNote = string.IsNullOrWhiteSpace(NoteText) ? null : NoteText.Trim();
		await SuggestionAdapter.UpdateAsync(suggestion);

		EditingNoteId = null;
		NoteText = string.Empty;
		await LoadSuggestionsAsync();
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

		if (EditingId.HasValue)
		{
			var suggestion = await SuggestionAdapter.GetByIdAsync(EditingId.Value);
			if (suggestion is not null)
			{
				suggestion.Category = NewCategory;
				suggestion.Description = NewDescription;
				suggestion.Title = NewTitle;
				await SuggestionAdapter.UpdateAsync(suggestion);
			}
		}
		else
		{
			await SuggestionAdapter.InsertAsync(ToNewSuggestion());
		}

		ShowSubmitModal = false;
		EditingId = null;
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
			"New" => filtered.OrderBy(s => s.StatusSortWeight).ThenByDescending(s => s.CreatedOn).ToList(),
			_ => filtered.OrderBy(s => s.StatusSortWeight).ThenByDescending(s => s.Votes).ToList()
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
			Status = SuggestionStatuses.New,
			Title = NewTitle,
			UserId = _userId!.Value,
		};
	}

	private SuggestionItem ToSuggestionItem(MODELS.Suggestion s, IEnumerable<MODELS.SuggestionVote> allVotes)
	{
		var votesForSuggestion = allVotes.Where(v => v.SuggestionId == s.Id).ToList();

		return new SuggestionItem {
			AdminNote = s.AdminNote,
			Category = s.Category,
			CreatedOn = s.CreatedOn,
			Description = s.Description,
			HasVoted = votesForSuggestion.Any(v => v.UserId == _userId),
			Id = s.Id,
			IsOwn = s.UserId == _userId,
			Status = s.Status,
			Title = s.Title,
			Votes = votesForSuggestion.Count,
		};
	}

	private List<SuggestionItem> Suggestions { get; set; } = [];

	#endregion
}
