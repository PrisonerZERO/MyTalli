namespace My.Talli.Web.ViewModels.Pages;

using Domain.Components.JsonSerializers;
using Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;
using Models;
using Services.Identity;
using Services.UI;
using System.Security.Claims;

/// <summary>View Model</summary>
public class DashboardViewModel : ComponentBase
{
    #region <Variables>

    [CascadingParameter]
    private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    [Inject]
    private IOptions<KnownIssueSettings> KnownIssueOptions { get; set; } = default!;

    [Inject]
    private UserPreferencesJsonSerializer PreferencesSerializer { get; set; } = default!;

    [Inject]
    private UserDisplayCache UserDisplayCache { get; set; } = default!;

    #endregion

    #region <Properties>

    public string ActivePeriod { get; private set; } = "30D";

    public string ActiveTab { get; private set; } = "overview";

    public string ChartCurrentAreaPath { get; private set; } = string.Empty;

    public string ChartCurrentLinePath { get; private set; } = string.Empty;

    public string ChartPreviousAreaPath { get; private set; } = string.Empty;

    public string ChartPreviousLinePath { get; private set; } = string.Empty;

    public double CircleStrokeDasharray { get; } = 364.42;

    public double CircleStrokeDashoffset => CircleStrokeDasharray * (1 - GoalPercentage / 100.0);

    public int DaysRemaining { get; private set; } = 12;

    public List<ExpenseItem> Expenses { get; private set; } = [];

    public decimal GoalCurrentAmount { get; private set; } = 1847m;

    public bool GoalOnTrack { get; private set; } = true;

    public int GoalPercentage { get; private set; } = 68;

    public decimal GoalTargetAmount { get; private set; } = 2700m;

    public bool IsSampleData { get; private set; } = true;

    public string KnownIssueMessage { get; private set; } = string.Empty;

    public string KnownIssueSeverity { get; private set; } = "Warning";

    public bool ShowKnownIssue { get; private set; }

    public int MonthlyGoalPercentage { get; private set; } = 68;

    public string PageTitle => ActiveTab == "overview" ? "Dashboard" : $"Dashboard — {ActiveTab[0].ToString().ToUpper()}{ActiveTab[1..]}";

    public List<PayoutItem> Payouts { get; private set; } = [];

    public List<string> Periods { get; } = ["7D", "30D", "90D", "12M"];

    public List<PlatformBreakdown> Platforms { get; private set; } = [];

    public int PlatformsConnected { get; private set; } = 3;

    public List<Transaction> RecentTransactions { get; private set; } = [];

    public string ThisMonthChange { get; private set; } = "12%";

    public decimal ThisMonthRevenue { get; private set; } = 1847m;

    public decimal TotalRevenue { get; private set; } = 4218m;

    public string TotalRevenueChange { get; private set; } = "23%";

    public string UserFirstName { get; private set; } = string.Empty;


    #endregion

    #region <Events>

    protected override async Task OnInitializedAsync()
    {
        LoadKnownIssue();
        await LoadUserFromClaims();
        LoadMockData();
    }


    #endregion

    #region <Methods>

    public void SelectPeriod(string period)
    {
        ActivePeriod = period;
        StateHasChanged();
    }

    public void SelectTab(string tab)
    {
        ActiveTab = tab;
    }

    private void LoadKnownIssue()
    {
        var settings = KnownIssueOptions.Value;
        ShowKnownIssue = settings.IsActive && !string.IsNullOrWhiteSpace(settings.Message);
        KnownIssueMessage = settings.Message;
        KnownIssueSeverity = settings.Severity;
    }

    private async Task LoadUserFromClaims()
    {
        var authState = await AuthenticationStateTask;
        var principal = authState.User;

        if (principal.Identity?.IsAuthenticated != true)
            return;

        var email = principal.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        var userIdClaim = principal.FindFirst("UserId")?.Value;

        if (userIdClaim is null || !long.TryParse(userIdClaim, out var userId))
            return;

        var (info, userPreferences) = await UserDisplayCache.GetOrLoadAsync(userId, email);

        UserFirstName = info.FirstName;

        var preferences = PreferencesSerializer.Deserialize(userPreferences);

        if (preferences.FunGreetings)
            UserFirstName = UserClaimsHelper.RandomFunGreeting();
    }

    private void LoadMockData()
    {
        ChartCurrentLinePath = DashboardDataset.GetChartCurrentLinePath();
        ChartCurrentAreaPath = ChartCurrentLinePath + " L796,200 L40,200 Z";
        ChartPreviousLinePath = DashboardDataset.GetChartPreviousLinePath();
        ChartPreviousAreaPath = ChartPreviousLinePath + " L796,200 L40,200 Z";
        Platforms = DashboardDataset.GetPlatforms();
        RecentTransactions = DashboardDataset.GetRecentTransactions();
        Expenses = ExpenseDataset.GetDashboardExpenses();
        Payouts = PayoutDataset.GetDashboardPayouts();
    }


    #endregion
}
