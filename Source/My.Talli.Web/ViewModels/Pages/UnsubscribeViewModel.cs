namespace My.Talli.Web.ViewModels.Pages;

using Domain.Components.JsonSerializers;
using Domain.Components.Tokens;
using Domain.Data.Interfaces;
using Domain.Models;
using Domain.Repositories;
using Microsoft.AspNetCore.Components;

using ENTITIES = Domain.Entities;

/// <summary>View Model</summary>
public class UnsubscribeViewModel : ComponentBase
{
    #region <Variables>

    [Inject]
    private ICurrentUserService CurrentUserService { get; set; } = default!;

    [Inject]
    private RepositoryAdapterAsync<User, ENTITIES.User> UserAdapter { get; set; } = default!;

    [Inject]
    private UnsubscribeTokenService TokenService { get; set; } = default!;

    [Inject]
    private UserPreferencesJsonSerializer PreferencesSerializer { get; set; } = default!;

    [SupplyParameterFromQuery(Name = "token")]
    private string? Token { get; set; }

    private long? _userId;


    #endregion

    #region <Properties>

    public bool IsLoading { get; private set; } = true;

    public bool IsSaved { get; private set; }

    public bool IsSaving { get; private set; }

    public bool IsValid { get; private set; }

    public bool SubscriptionConfirmationEmail { get; set; } = true;

    public bool UnsubscribeAll { get; set; }

    public string UserFirstName { get; private set; } = string.Empty;

    public bool WeeklySummaryEmail { get; set; } = true;


    #endregion

    #region <Events>

    protected override async Task OnInitializedAsync()
    {
        _userId = TokenService.ValidateToken(Token);

        if (_userId is null)
        {
            IsLoading = false;
            return;
        }

        var user = await UserAdapter.GetByIdAsync(_userId.Value);

        if (user is null)
        {
            IsLoading = false;
            return;
        }

        IsValid = true;
        UserFirstName = user.FirstName;

        var preferences = PreferencesSerializer.Deserialize(user.UserPreferences);
        SubscriptionConfirmationEmail = preferences.EmailPreferences.SubscriptionConfirmationEmail;
        UnsubscribeAll = preferences.EmailPreferences.UnsubscribeAll;
        WeeklySummaryEmail = preferences.EmailPreferences.WeeklySummaryEmail;

        IsLoading = false;
    }


    #endregion

    #region <Methods>

    public async Task SavePreferencesAsync()
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

        CurrentUserService.Set(_userId.Value, user.DisplayName);

        var preferences = PreferencesSerializer.Deserialize(user.UserPreferences);
        preferences.EmailPreferences.SubscriptionConfirmationEmail = SubscriptionConfirmationEmail;
        preferences.EmailPreferences.UnsubscribeAll = UnsubscribeAll;
        preferences.EmailPreferences.WeeklySummaryEmail = WeeklySummaryEmail;
        user.UserPreferences = PreferencesSerializer.Serialize(preferences);

        await UserAdapter.UpdateAsync(user);

        IsSaving = false;
        IsSaved = true;
    }

    public void OnToggleChanged()
    {
        IsSaved = false;
    }


    #endregion
}
