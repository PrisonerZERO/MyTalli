namespace My.Talli.UnitTesting.Components.Stripe;

using Domain.Components;

/// <summary>Tests</summary>
public class StripeOAuthGeneratorTests
{
    #region <Methods>

    [Fact]
    public void BuildAuthorizeChallenge_AlwaysProducesNonEmptyState()
    {
        var challenge = StripeOAuthGenerator.BuildAuthorizeChallenge("ca_test", "https://localhost/cb", "read_only");

        Assert.False(string.IsNullOrEmpty(challenge.State));
    }

    [Fact]
    public void BuildAuthorizeChallenge_LeavesCodeVerifierEmpty()
    {
        var challenge = StripeOAuthGenerator.BuildAuthorizeChallenge("ca_test", "https://localhost/cb", "read_only");

        Assert.Equal(string.Empty, challenge.CodeVerifier);
    }

    [Fact]
    public void BuildAuthorizeChallenge_AuthorizeUrlPointsAtStripeConnect()
    {
        var challenge = StripeOAuthGenerator.BuildAuthorizeChallenge("ca_test", "https://localhost/cb", "read_only");

        Assert.StartsWith("https://connect.stripe.com/oauth/authorize", challenge.AuthorizeUrl);
    }

    [Fact]
    public void BuildAuthorizeChallenge_IncludesClientIdRedirectScopeAndState()
    {
        var challenge = StripeOAuthGenerator.BuildAuthorizeChallenge("ca_my_client", "https://localhost:7012/cb", "read_only");

        Assert.Contains("client_id=ca_my_client", challenge.AuthorizeUrl);
        Assert.Contains("redirect_uri=https%3A%2F%2Flocalhost%3A7012%2Fcb", challenge.AuthorizeUrl);
        Assert.Contains("scope=read_only", challenge.AuthorizeUrl);
        Assert.Contains($"state={Uri.EscapeDataString(challenge.State)}", challenge.AuthorizeUrl);
    }

    [Fact]
    public void BuildAuthorizeChallenge_DoesNotIncludeCodeChallenge()
    {
        var challenge = StripeOAuthGenerator.BuildAuthorizeChallenge("ca_test", "https://localhost/cb", "read_only");

        Assert.DoesNotContain("code_challenge", challenge.AuthorizeUrl);
    }

    [Fact]
    public void BuildAuthorizeChallenge_StateIsRandomAcrossCalls()
    {
        var first = StripeOAuthGenerator.BuildAuthorizeChallenge("ca", "https://localhost/cb", "read_only");
        var second = StripeOAuthGenerator.BuildAuthorizeChallenge("ca", "https://localhost/cb", "read_only");

        Assert.NotEqual(first.State, second.State);
    }

    #endregion
}
