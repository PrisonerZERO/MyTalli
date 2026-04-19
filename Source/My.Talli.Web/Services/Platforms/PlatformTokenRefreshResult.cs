namespace My.Talli.Web.Services.Platforms;

/// <summary>Result</summary>
public class PlatformTokenRefreshResult
{
    #region <Properties>

    public string AccessToken { get; set; } = string.Empty;

    public DateTime AccessTokenExpiryDateTime { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime RefreshTokenExpiryDateTime { get; set; }

    #endregion
}
