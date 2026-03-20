namespace My.Talli.Web.ViewModels.Pages;

using Domain.Framework;
using Domain.Repositories;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>View Model</summary>
public class WaitlistViewModel : ComponentBase
{
    #region <Variables>

    [CascadingParameter]
    private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    [Inject]
    private RepositoryAdapterAsync<MODELS.Milestone, ENTITIES.Milestone> MilestoneRepository { get; set; } = default!;


    #endregion

    #region <Properties>

    public List<MODELS.Milestone> BetaMilestones { get; private set; } = [];

    public List<MODELS.Milestone> FullLaunchMilestones { get; private set; } = [];

    public string UserName { get; private set; } = string.Empty;


    #endregion

    #region <Events>

    protected override async Task OnInitializedAsync()
    {
        await LoadMilestonesAsync();

        if (AuthenticationStateTask is not null)
        {
            var authState = await AuthenticationStateTask;
            var user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                UserName = user.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            }
        }
    }


    #endregion

    #region <Methods>

    private async Task LoadMilestonesAsync()
    {
        var milestones = await MilestoneRepository.FindAsync(m => !m.IsDeleted);

        BetaMilestones = milestones
            .Where(m => m.MilestoneGroup == MilestoneGroups.Beta)
            .OrderBy(m => m.SortOrder)
            .ToList();

        FullLaunchMilestones = milestones
            .Where(m => m.MilestoneGroup == MilestoneGroups.FullLaunch)
            .OrderBy(m => m.SortOrder)
            .ToList();
    }


    #endregion
}
