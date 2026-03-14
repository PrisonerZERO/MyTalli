namespace My.Talli.Domain.Data.Interfaces;

/// <summary>Provides the identity of the currently authenticated user for audit stamping.</summary>
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
