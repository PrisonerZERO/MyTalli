namespace My.Talli.Web.Services.Platforms;

/// <summary>Service</summary>
public interface IPlatformTokenRefresher
{
    #region <Properties>

    string Platform { get; }

    TimeSpan ProactiveRefreshWindow { get; }

    #endregion

    #region <Methods>

    Task<PlatformTokenRefreshResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken);

    #endregion
}
