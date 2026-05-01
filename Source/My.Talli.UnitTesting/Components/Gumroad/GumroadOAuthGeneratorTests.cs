namespace My.Talli.UnitTesting.Components.Gumroad;

using Domain.Components;

/// <summary>Tests</summary>
public class GumroadOAuthGeneratorTests
{
    #region <Methods>

    [Fact]
    public void BuildAuthorizeChallenge_AlwaysProducesNonEmptyState()
    {
        var challenge = GumroadOAuthGenerator.BuildAuthorizeChallenge("client", "https://localhost/cb", "view_sales");

        Assert.False(string.IsNullOrEmpty(challenge.State));
    }

    [Fact]
    public void BuildAuthorizeChallenge_LeavesCodeVerifierEmpty()
    {
        var challenge = GumroadOAuthGenerator.BuildAuthorizeChallenge("client", "https://localhost/cb", "view_sales");

        Assert.Equal(string.Empty, challenge.CodeVerifier);
    }

    [Fact]
    public void BuildAuthorizeChallenge_AuthorizeUrlPointsAtGumroad()
    {
        var challenge = GumroadOAuthGenerator.BuildAuthorizeChallenge("client", "https://localhost/cb", "view_sales");

        Assert.StartsWith("https://gumroad.com/oauth/authorize", challenge.AuthorizeUrl);
    }

    [Fact]
    public void BuildAuthorizeChallenge_IncludesClientIdRedirectScopeAndState()
    {
        var challenge = GumroadOAuthGenerator.BuildAuthorizeChallenge("my-client", "https://localhost:7012/cb", "view_sales");

        Assert.Contains("client_id=my-client", challenge.AuthorizeUrl);
        Assert.Contains("redirect_uri=https%3A%2F%2Flocalhost%3A7012%2Fcb", challenge.AuthorizeUrl);
        Assert.Contains("scope=view_sales", challenge.AuthorizeUrl);
        Assert.Contains($"state={Uri.EscapeDataString(challenge.State)}", challenge.AuthorizeUrl);
    }

    [Fact]
    public void BuildAuthorizeChallenge_DoesNotIncludeCodeChallenge()
    {
        var challenge = GumroadOAuthGenerator.BuildAuthorizeChallenge("client", "https://localhost/cb", "view_sales");

        Assert.DoesNotContain("code_challenge", challenge.AuthorizeUrl);
    }

    [Fact]
    public void BuildAuthorizeChallenge_StateIsRandomAcrossCalls()
    {
        var first = GumroadOAuthGenerator.BuildAuthorizeChallenge("c", "https://localhost/cb", "view_sales");
        var second = GumroadOAuthGenerator.BuildAuthorizeChallenge("c", "https://localhost/cb", "view_sales");

        Assert.NotEqual(first.State, second.State);
    }

    #endregion
}
