namespace My.Talli.Web.ViewModels.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

/// <summary>View Model</summary>
public class WaitlistViewModel : ComponentBase
{
    #region <Variables>

    [CascadingParameter]
    private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    #endregion

    #region <Properties>

    public List<Milestone> BetaMilestones { get; private set; } = [];

    public List<Milestone> FullLaunchMilestones { get; private set; } = [];

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
        BetaMilestones =
        [
            new Milestone(
                "Authentication",
                "Sign in with Google, Apple, and Microsoft",
                MilestoneStatus.Complete),
            new Milestone(
                "Landing Page & Waitlist",
                "Landing page and waitlist progress tracker",
                MilestoneStatus.Complete),
            new Milestone(
                "Stripe Integration",
                "Connector & Dashboard",
                MilestoneStatus.Upcoming),
            new Milestone(
                "Etsy Integration",
                "Connector & Dashboard",
                MilestoneStatus.Upcoming),
            new Milestone(
                "Gumroad Integration",
                "Connector & Dashboard",
                MilestoneStatus.Upcoming),
            new Milestone(
                "Aggregate Dashboard",
                "Unified revenue view across all connected platforms",
                MilestoneStatus.Upcoming),
            new Milestone(
                "Platforms",
                "Manage connected platforms and sync status",
                MilestoneStatus.Upcoming),
            new Milestone(
                "Suggestion Box",
                "Submit and vote on feature ideas",
                MilestoneStatus.Upcoming),
            new Milestone(
                "Settings",
                "User profile and preferences",
                MilestoneStatus.Upcoming),
            new Milestone(
                "Subscription & Billing",
                "Free and Pro tier management",
                MilestoneStatus.Upcoming),
            new Milestone(
                "Beta Launch",
                "Early access for waitlist members — you'll be first in line",
                MilestoneStatus.Upcoming)
        ];

        FullLaunchMilestones =
        [
            new Milestone(
                "PayPal Integration",
                "Connector & Dashboard",
                MilestoneStatus.Upcoming),
            new Milestone(
                "Shopify Integration",
                "Connector & Dashboard",
                MilestoneStatus.Upcoming),
            new Milestone(
                "Goals",
                "Set monthly revenue targets and track progress",
                MilestoneStatus.Upcoming),
            new Milestone(
                "CSV Export",
                "Download revenue data for tax prep and bookkeeping",
                MilestoneStatus.Upcoming),
            new Milestone(
                "Weekly Email Summaries",
                "Weekly revenue digest via email",
                MilestoneStatus.Upcoming),
            new Milestone(
                "Full Launch",
                "Open to everyone",
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
