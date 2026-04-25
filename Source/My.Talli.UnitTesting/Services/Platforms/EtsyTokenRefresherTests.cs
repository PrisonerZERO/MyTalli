namespace My.Talli.UnitTesting.Services.Platforms;

using Domain.Components;
using My.Talli.UnitTesting.Infrastructure.Stubs;
using My.Talli.Web.Services.Platforms;

/// <summary>Tests</summary>
public class EtsyTokenRefresherTests
{
    #region <Methods>

    [Fact]
    public void Platform_IsEtsy()
    {
        var refresher = new EtsyTokenRefresher(new EtsyApiClientStub());

        Assert.Equal("Etsy", refresher.Platform);
    }

    [Fact]
    public void ProactiveRefreshWindow_Is30Days()
    {
        var refresher = new EtsyTokenRefresher(new EtsyApiClientStub());

        Assert.Equal(TimeSpan.FromDays(30), refresher.ProactiveRefreshWindow);
    }

    [Fact]
    public async Task Refresh_PassesRefreshTokenToApiClient()
    {
        var stub = new EtsyApiClientStub
        {
            RefreshResponse = BuildTokenResponse()
        };
        var refresher = new EtsyTokenRefresher(stub);

        await refresher.RefreshAsync("rt-old", CancellationToken.None);

        Assert.Single(stub.RefreshCalls);
        Assert.Equal("rt-old", stub.RefreshCalls[0]);
    }

    [Fact]
    public async Task Refresh_ReturnsAccessTokenFromApi()
    {
        var stub = new EtsyApiClientStub
        {
            RefreshResponse = BuildTokenResponse(accessToken: "new-access")
        };
        var refresher = new EtsyTokenRefresher(stub);

        var result = await refresher.RefreshAsync("rt", CancellationToken.None);

        Assert.Equal("new-access", result.AccessToken);
    }

    [Fact]
    public async Task Refresh_AccessTokenExpiry_IsNowPlusExpiresInSeconds()
    {
        var stub = new EtsyApiClientStub
        {
            RefreshResponse = BuildTokenResponse(expiresIn: 3600)
        };
        var refresher = new EtsyTokenRefresher(stub);

        var before = DateTime.UtcNow;
        var result = await refresher.RefreshAsync("rt", CancellationToken.None);
        var after = DateTime.UtcNow;

        Assert.InRange(result.AccessTokenExpiryDateTime, before.AddSeconds(3600 - 1), after.AddSeconds(3600 + 1));
    }

    [Fact]
    public async Task Refresh_RefreshTokenExpiry_Is90DaysFromNow()
    {
        var stub = new EtsyApiClientStub
        {
            RefreshResponse = BuildTokenResponse()
        };
        var refresher = new EtsyTokenRefresher(stub);

        var before = DateTime.UtcNow;
        var result = await refresher.RefreshAsync("rt", CancellationToken.None);
        var after = DateTime.UtcNow;

        Assert.InRange(result.RefreshTokenExpiryDateTime, before.AddDays(90).AddSeconds(-1), after.AddDays(90).AddSeconds(1));
    }

    [Fact]
    public async Task Refresh_PropagatesRotatedRefreshToken()
    {
        var stub = new EtsyApiClientStub
        {
            RefreshResponse = BuildTokenResponse(refreshToken: "rt-new")
        };
        var refresher = new EtsyTokenRefresher(stub);

        var result = await refresher.RefreshAsync("rt-old", CancellationToken.None);

        Assert.Equal("rt-new", result.RefreshToken);
    }

    private static EtsyTokenResponse BuildTokenResponse(string accessToken = "access", string refreshToken = "refresh", int expiresIn = 3600)
    {
        return new EtsyTokenResponse
        {
            AccessToken = accessToken,
            ExpiresIn = expiresIn,
            RefreshToken = refreshToken,
            TokenType = "Bearer"
        };
    }

    #endregion
}
