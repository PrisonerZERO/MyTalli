namespace My.Talli.Web.ViewModels.Pages;

using Domain.Components.JsonSerializers;
using Domain.Data.Interfaces;
using Domain.Models;
using Domain.Repositories;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Services.Identity;
using System.Security.Claims;

using ENTITIES = Domain.Entities;

/// <summary>View Model</summary>
public class SettingsViewModel : ComponentBase
{
    #region <Variables>

    [CascadingParameter]
    private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    [Inject]
    private ICurrentUserService CurrentUserService { get; set; } = default!;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    [Inject]
    private UserPreferencesJsonSerializer PreferencesSerializer { get; set; } = default!;

    [Inject]
    private RepositoryAdapterAsync<User, ENTITIES.User> UserAdapter { get; set; } = default!;

    [Inject]
    private UserDisplayCache UserDisplayCache { get; set; } = default!;

    private long? _userId;


    #endregion

    #region <Properties>

    public string DarkMode { get; set; } = "system";

    public string DisplayName { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public bool FunGreetings { get; set; } = true;

    public bool IsLoading { get; private set; } = true;

    public bool IsSaved { get; private set; }

    public bool IsSaving { get; private set; }

    public string LastName { get; set; } = string.Empty;

    public bool SubscriptionConfirmationEmail { get; set; } = true;

    public bool UnsubscribeAll { get; set; }

    public bool WeeklySummaryEmail { get; set; } = true;


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

        var user = await UserAdapter.GetByIdAsync(userId);

        if (user is null)
            return;

        DisplayName = user.DisplayName;
        FirstName = user.FirstName;
        LastName = user.LastName;

        var preferences = PreferencesSerializer.Deserialize(user.UserPreferences);
        DarkMode = preferences.DarkMode;
        FunGreetings = preferences.FunGreetings;
        SubscriptionConfirmationEmail = preferences.EmailPreferences.SubscriptionConfirmationEmail;
        UnsubscribeAll = preferences.EmailPreferences.UnsubscribeAll;
        WeeklySummaryEmail = preferences.EmailPreferences.WeeklySummaryEmail;

        IsLoading = false;
    }


    #endregion

    #region <Methods>

    public void OnFieldChanged()
    {
        IsSaved = false;
    }

    public async Task SetThemeAsync(string mode)
    {
        DarkMode = mode;
        IsSaved = false;
        await JsRuntime.InvokeVoidAsync("themeManager.apply", mode);
        await JsRuntime.InvokeVoidAsync("eval", $"document.documentElement.setAttribute('data-theme-mode', '{mode}')");
    }

    public async Task SaveSettingsAsync()
    {
        if (_userId is null || IsSaving)
            return;

        IsSaving = true;
        IsSaved = false;

        var user = await UserAdapter.GetByIdAsync(_userId.Value);

        if (user is null)
        {
            IsSaving = false;
            return;
        }

        CurrentUserService.Set(_userId.Value, DisplayName);

        user.DisplayName = DisplayName;
        user.FirstName = FirstName;
        user.LastName = LastName;

        var preferences = PreferencesSerializer.Deserialize(user.UserPreferences);
        preferences.DarkMode = DarkMode;
        preferences.FunGreetings = FunGreetings;
        preferences.EmailPreferences.SubscriptionConfirmationEmail = SubscriptionConfirmationEmail;
        preferences.EmailPreferences.UnsubscribeAll = UnsubscribeAll;
        preferences.EmailPreferences.WeeklySummaryEmail = WeeklySummaryEmail;
        user.UserPreferences = PreferencesSerializer.Serialize(preferences);

        await UserAdapter.UpdateAsync(user);

        UserDisplayCache.Invalidate();

        IsSaving = false;
        IsSaved = true;
    }


    #endregion
}
