namespace My.Talli.Web.ViewModels.Pages;

using Commands.Endpoints;
using Commands.Notifications;
using Domain.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

/// <summary>View Model</summary>
public class AdminViewModel : ComponentBase
{
    #region <Variables>

    [CascadingParameter]
    private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    [Inject]
    private GetAdminUserListCommand AdminUserListCommand { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private SendSubscriptionConfirmationEmailCommand SubscriptionConfirmationCommand { get; set; } = default!;

    [Inject]
    private SendWelcomeEmailCommand WelcomeCommand { get; set; } = default!;

    [Inject]
    private SendWeeklySummaryEmailCommand WeeklySummaryCommand { get; set; } = default!;

    #endregion

    #region <Properties>

    public List<AdminUserListItem> FilteredUsers { get; private set; } = [];

    public bool IsConfirmingBulkAll { get; private set; }

    public bool IsConfirmingBulkSelected { get; private set; }

    public bool IsLoading { get; private set; } = true;

    public bool IsSending { get; private set; }

    public string SearchText { get; set; } = string.Empty;

    public bool SelectAll { get; private set; }

    public HashSet<long> SelectedUserIds { get; private set; } = [];

    public AdminUserListItem? SelectedUser { get; private set; }

    public bool IsStatusSuccess { get; private set; }

    public string? StatusMessage { get; private set; }

    public List<AdminUserListItem> Users { get; private set; } = [];


    #endregion

    #region <Events>

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateTask;
        var principal = authState.User;

        if (principal.Identity?.IsAuthenticated != true || !principal.IsInRole("Admin"))
        {
            NavigationManager.NavigateTo("/dashboard");
            return;
        }

        Users = await AdminUserListCommand.ExecuteAsync();
        FilteredUsers = Users;
        IsLoading = false;
    }


    #endregion

    #region <Methods>

    public void CancelBulk()
    {
        IsConfirmingBulkAll = false;
        IsConfirmingBulkSelected = false;
    }

    public async Task ConfirmBulkSendAllAsync()
    {
        IsConfirmingBulkAll = false;
        IsSending = true;
        StatusMessage = null;

        try
        {
            var sent = 0;

            foreach (var user in Users)
            {
                await WelcomeCommand.ExecuteAsync(user.Email, user.FirstName, user.UserId);
                sent++;
            }

            StatusMessage = $"Welcome email sent to {sent} user(s).";
            IsStatusSuccess = true;
        }
        catch (Exception)
        {
            StatusMessage = "An error occurred while sending emails.";
            IsStatusSuccess = false;
        }

        IsSending = false;
    }

    public async Task ConfirmBulkSendSelectedAsync()
    {
        IsConfirmingBulkSelected = false;
        IsSending = true;
        StatusMessage = null;

        try
        {
            var targetUsers = Users.Where(u => SelectedUserIds.Contains(u.UserId)).ToList();
            var sent = 0;

            foreach (var user in targetUsers)
            {
                await WelcomeCommand.ExecuteAsync(user.Email, user.FirstName, user.UserId);
                sent++;
            }

            StatusMessage = $"Welcome email sent to {sent} user(s).";
            IsStatusSuccess = true;
        }
        catch (Exception)
        {
            StatusMessage = "An error occurred while sending emails.";
            IsStatusSuccess = false;
        }

        IsSending = false;
    }

    public void DismissStatus()
    {
        StatusMessage = null;
    }

    public void FilterUsers()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            FilteredUsers = Users;
            return;
        }

        var search = SearchText.Trim();

        FilteredUsers = Users.Where(u =>
            u.DisplayName.Contains(search, StringComparison.OrdinalIgnoreCase)
            || u.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase)
            || u.Email.Contains(search, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public async Task ResendEmailAsync(string emailType)
    {
        if (SelectedUser is null)
            return;

        IsSending = true;
        StatusMessage = null;

        try
        {
            switch (emailType)
            {
                case "welcome":
                    await WelcomeCommand.ExecuteAsync(SelectedUser.Email, SelectedUser.FirstName, SelectedUser.UserId);
                    StatusMessage = $"Welcome email sent to {SelectedUser.Email}.";
                    IsStatusSuccess = true;
                    break;

                case "subscription-confirmation":
                    var sent = await SubscriptionConfirmationCommand.ExecuteAsync(SelectedUser.Email, SelectedUser.FirstName, SelectedUser.UserId);
                    StatusMessage = sent
                        ? $"Subscription confirmation sent to {SelectedUser.Email}."
                        : "No subscription data found for this user.";
                    IsStatusSuccess = sent;
                    break;

                case "weekly-summary":
                    await WeeklySummaryCommand.ExecuteAsync(SelectedUser.Email, SelectedUser.FirstName, SelectedUser.UserId);
                    StatusMessage = $"Weekly summary sent to {SelectedUser.Email}.";
                    IsStatusSuccess = true;
                    break;
            }
        }
        catch (Exception)
        {
            StatusMessage = $"Failed to send {emailType} email.";
            IsStatusSuccess = false;
        }

        IsSending = false;
    }

    public void SelectUser(AdminUserListItem user)
    {
        SelectedUser = user;
        StatusMessage = null;
    }

    public void ShowBulkAllConfirmation()
    {
        IsConfirmingBulkAll = true;
        IsConfirmingBulkSelected = false;
    }

    public void ShowBulkSelectedConfirmation()
    {
        IsConfirmingBulkSelected = true;
        IsConfirmingBulkAll = false;
    }

    public void ToggleSelectAll()
    {
        SelectAll = !SelectAll;

        if (SelectAll)
            SelectedUserIds = new HashSet<long>(FilteredUsers.Select(u => u.UserId));
        else
            SelectedUserIds.Clear();
    }

    public void OnUserSelected(ChangeEventArgs e)
    {
        if (long.TryParse(e.Value?.ToString(), out var userId))
        {
            var user = Users.FirstOrDefault(u => u.UserId == userId);

            if (user is not null)
                SelectUser(user);
        }
        else
        {
            SelectedUser = null;
            StatusMessage = null;
        }
    }

    public static string ResolveDisplayName(AdminUserListItem user) =>
        !string.IsNullOrWhiteSpace(user.DisplayName) ? user.DisplayName
        : !string.IsNullOrWhiteSpace(user.FirstName) ? user.FirstName
        : user.Email.Split('@')[0];

    public void ToggleUserSelection(long userId)
    {
        if (!SelectedUserIds.Add(userId))
            SelectedUserIds.Remove(userId);

        SelectAll = SelectedUserIds.Count == FilteredUsers.Count && FilteredUsers.Count > 0;
    }

    #endregion
}
