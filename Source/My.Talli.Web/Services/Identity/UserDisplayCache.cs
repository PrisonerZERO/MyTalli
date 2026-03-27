namespace My.Talli.Web.Services.Identity;

using Domain.Models;
using Domain.Repositories;
using Helpers;

using ENTITIES = Domain.Entities;

/// <summary>Scoped cache for user display info — serializes DB access across concurrent Blazor components</summary>
public class UserDisplayCache
{
    #region <Variables>

    private readonly SemaphoreSlim _lock = new(1, 1); //<-- Acts as an async-friendly mutex — only one caller can be inside at a time.
    private readonly RepositoryAdapterAsync<User, ENTITIES.User> _userAdapter;

    private string? _cachedPreferences;
    private UserDisplayInfo? _cachedInfo;

    #endregion

    #region <Constructors>

    public UserDisplayCache(RepositoryAdapterAsync<User, ENTITIES.User> userAdapter)
    {
        _userAdapter = userAdapter;
    }

    #endregion

    #region <Methods>

    public async Task<(UserDisplayInfo Info, string Preferences)> GetOrLoadAsync(long userId, string email)
    {
        await _lock.WaitAsync();

        try
        {
            if (_cachedInfo is not null)
                return (_cachedInfo, _cachedPreferences!);

            var user = await _userAdapter.GetByIdAsync(userId);

            if (user is null)
            {
                _cachedInfo = UserClaimsHelper.Resolve(string.Empty, string.Empty, string.Empty, email);
                _cachedPreferences = "{}";

                return (_cachedInfo, _cachedPreferences);
            }

            _cachedInfo = UserClaimsHelper.Resolve(user.FirstName, user.LastName, user.DisplayName, email);
            _cachedPreferences = user.UserPreferences;

            return (_cachedInfo, _cachedPreferences);
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Invalidate()
    {
        _cachedInfo = null;
        _cachedPreferences = null;
    }

    #endregion
}
