namespace My.Talli.Web.Services.Platforms;

/// <summary>Refresher</summary>
public class EtsyTokenRefresher : IPlatformTokenRefresher
{
    #region <Constants>

    private const int EtsyRefreshTokenLifetimeDays = 90;

    #endregion

    #region <Variables>

    private readonly EtsyService _etsyService;

    #endregion

    #region <Constructors>

    public EtsyTokenRefresher(EtsyService etsyService)
    {
        _etsyService = etsyService;
    }

    #endregion

    #region <Properties>

    public string Platform => "Etsy";

    public TimeSpan ProactiveRefreshWindow => TimeSpan.FromDays(30);

    #endregion

    #region <Methods>

    public async Task<PlatformTokenRefreshResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var tokens = await _etsyService.RefreshTokensAsync(refreshToken, cancellationToken);
        var now = DateTime.UtcNow;

        return new PlatformTokenRefreshResult
        {
            AccessToken = tokens.AccessToken,
            AccessTokenExpiryDateTime = now.AddSeconds(tokens.ExpiresIn),
            RefreshToken = tokens.RefreshToken,
            RefreshTokenExpiryDateTime = now.AddDays(EtsyRefreshTokenLifetimeDays)
        };
    }

    #endregion
}
