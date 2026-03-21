namespace My.Talli.Web.ViewModels.Pages;

using Domain.Components.JsonSerializers;
using Domain.Models;
using Domain.Repositories;
using Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

using ENTITIES = Domain.Entities;

/// <summary>View Model</summary>
public class DashboardViewModel : ComponentBase
{
    #region <Variables>

    [CascadingParameter]
    private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    [Inject]
    private UserPreferencesJsonSerializer PreferencesSerializer { get; set; } = default!;

    [Inject]
    private RepositoryAdapterAsync<User, ENTITIES.User> UserAdapter { get; set; } = default!;

    #endregion

    #region <Properties>

    public string ActivePeriod { get; private set; } = "30D";

    public string ChartCurrentAreaPath { get; private set; } = string.Empty;

    public string ChartCurrentLinePath { get; private set; } = string.Empty;

    public string ChartPreviousAreaPath { get; private set; } = string.Empty;

    public string ChartPreviousLinePath { get; private set; } = string.Empty;

    public double CircleStrokeDasharray { get; } = 364.42;

    public double CircleStrokeDashoffset => CircleStrokeDasharray * (1 - GoalPercentage / 100.0);

    public int DaysRemaining { get; private set; } = 12;

    public decimal GoalCurrentAmount { get; private set; } = 1847m;

    public bool GoalOnTrack { get; private set; } = true;

    public int GoalPercentage { get; private set; } = 68;

    public decimal GoalTargetAmount { get; private set; } = 2700m;

    public bool IsSampleData { get; private set; } = true;

    public bool IsUserMenuOpen { get; private set; }

    public int MonthlyGoalPercentage { get; private set; } = 68;

    public List<string> Periods { get; } = ["7D", "30D", "90D", "12M"];

    public List<PlatformBreakdown> Platforms { get; private set; } = [];

    public int PlatformsConnected { get; private set; } = 3;

    public List<Transaction> RecentTransactions { get; private set; } = [];

    public string ThisMonthChange { get; private set; } = "12%";

    public decimal ThisMonthRevenue { get; private set; } = 1847m;

    public decimal TotalRevenue { get; private set; } = 4218m;

    public string TotalRevenueChange { get; private set; } = "23%";

    public string UserEmail { get; private set; } = string.Empty;

    public string UserFirstName { get; private set; } = string.Empty;

    public string UserFullName { get; private set; } = string.Empty;

    public string UserInitials { get; private set; } = string.Empty;


    #endregion

    #region <Events>

    protected override async Task OnInitializedAsync()
    {
        await LoadUserFromClaims();
        LoadMockData();
    }


    #endregion

    #region <Methods>

    public void CloseUserMenu()
    {
        IsUserMenuOpen = false;
    }

    public void SelectPeriod(string period)
    {
        ActivePeriod = period;
        StateHasChanged();
    }

    public void ToggleUserMenu()
    {
        IsUserMenuOpen = !IsUserMenuOpen;
    }

    private async Task LoadUserFromClaims()
    {
        var authState = await AuthenticationStateTask;
        var principal = authState.User;

        if (principal.Identity?.IsAuthenticated != true)
            return;

        var info = UserClaimsHelper.Resolve(principal);

        UserEmail = info.Email;
        UserFirstName = info.FirstName;
        UserFullName = info.FullName;
        UserInitials = info.Initials;

        var userIdClaim = principal.FindFirst("UserId")?.Value;

        if (userIdClaim is not null && long.TryParse(userIdClaim, out var userId))
        {
            var user = await UserAdapter.GetByIdAsync(userId);

            if (user is not null)
            {
                var preferences = PreferencesSerializer.Deserialize(user.UserPreferences);

                if (preferences.FunGreetings)
                    UserFirstName = UserClaimsHelper.RandomFunGreeting();
            }
        }
    }

    private void LoadMockData()
    {
        ChartCurrentLinePath = "M40,155 L67,148 L94,152 L121,140 L148,145 L175,130 L202,138 L229,125 L256,130 L283,118 L310,124 L337,108 L364,100 L391,105 L418,92 L445,98 L472,85 L499,78 L526,82 L553,70 L580,75 L607,62 L634,68 L661,55 L688,50 L715,45 L742,52 L769,38 L796,32";

        ChartCurrentAreaPath = ChartCurrentLinePath + " L796,200 L40,200 Z";

        ChartPreviousLinePath = "M40,140 L67,135 L94,142 L121,130 L148,138 L175,125 L202,132 L229,120 L256,128 L283,115 L310,122 L337,110 L364,118 L391,105 L418,112 L445,100 L472,108 L499,95 L526,102 L553,90 L580,97 L607,85 L634,92 L661,80 L688,88 L715,75 L742,82 L769,70 L796,78";

        ChartPreviousAreaPath = ChartPreviousLinePath + " L796,200 L40,200 Z";

        Platforms =
        [
            new PlatformBreakdown("Stripe", "#635bff", 2340m, 55),
            new PlatformBreakdown("Etsy", "#f56400", 1128m, 27),
            new PlatformBreakdown("Gumroad", "#ff90e8", 750m, 18)
        ];

        RecentTransactions =
        [
            new Transaction("Etsy", "#f56400", "Handmade Candle Set (x2)", "Mar 1", 68.00m),
            new Transaction("Stripe", "#635bff", "Logo Design — Freelance", "Feb 28", 450.00m),
            new Transaction("Gumroad", "#ff90e8", "Procreate Brush Pack", "Feb 27", 12.00m),
            new Transaction("Stripe", "#635bff", "Web Dev Retainer — Feb", "Feb 26", 800.00m),
            new Transaction("Etsy", "#f56400", "Ceramic Mug — Speckled", "Feb 25", 34.00m)
        ];
    }


    #endregion
}

public record PlatformBreakdown(string Name, string Color, decimal Amount, int Percentage);

public record Transaction(string PlatformName, string PlatformColor, string Description, string Date, decimal Amount);
