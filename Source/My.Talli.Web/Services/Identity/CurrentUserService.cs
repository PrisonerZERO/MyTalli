namespace My.Talli.Web.Services.Identity;

using My.Talli.Domain.Data.Interfaces;

/// <summary>Identity</summary>
public class CurrentUserService : ICurrentUserService
{
    #region <Properties>

    public string? DisplayName { get; private set; }

    public bool IsAuthenticated => UserId.HasValue;

    public long? UserId { get; private set; }

    #endregion

    #region <Methods>

    public void Clear()
    {
        DisplayName = null;
        UserId = null;
    }

    public void Set(long userId, string displayName)
    {
        UserId = userId;
        DisplayName = displayName;
    }

    #endregion
}
