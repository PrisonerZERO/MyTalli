using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace My.Talli.Web.ViewModels.Pages;

public class WaitlistViewModel : ComponentBase
{
    #region <Variables>

    [CascadingParameter]
    private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    #endregion

    #region <Properties>

    public List<Milestone> Milestones { get; private set; } = [];

    public string UserName { get; private set; } = string.Empty;

    #endregion

    #region <Events>

    protected override async Task OnInitializedAsync()
    {
        LoadMilestones();

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

    private void LoadMilestones()
    {
        Milestones =
        [
            new Milestone(
                "Authentication",
                "Sign in with Google, Apple, and Microsoft",
                MilestoneStatus.Complete),
            new Milestone(
                "Dashboard & Revenue Tracking",
                "Unified view of all your side hustle income",
                MilestoneStatus.Complete),
            new Milestone(
                "Stripe Connector",
                "Connect your Stripe account and pull revenue data automatically",
                MilestoneStatus.InProgress),
            new Milestone(
                "Etsy Connector",
                "Pull sales data from your Etsy shop",
                MilestoneStatus.Upcoming),
            new Milestone(
                "Gumroad Connector",
                "Track digital product revenue from Gumroad",
                MilestoneStatus.Upcoming),
            new Milestone(
                "Goals & CSV Export",
                "Set monthly targets and export data for tax prep",
                MilestoneStatus.Upcoming),
            new Milestone(
                "Beta Launch",
                "Early access for waitlist members — you'll be first in line",
                MilestoneStatus.Upcoming)
        ];
    }

    #endregion
}

public enum MilestoneStatus
{
    Complete,
    InProgress,
    Upcoming
}

public record Milestone(string Title, string Description, MilestoneStatus Status);
