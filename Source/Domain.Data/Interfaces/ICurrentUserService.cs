namespace My.Talli.Domain.Data.Interfaces;

/// <summary>Identity</summary>
public interface ICurrentUserService
{
    #region <Properties>

    long? UserId { get; }

    string? DisplayName { get; }

    bool IsAuthenticated { get; }


    #endregion

    #region <Methods>

    void Set(long userId, string displayName);

    void Clear();


    #endregion
}
