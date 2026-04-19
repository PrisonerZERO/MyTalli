namespace My.Talli.UnitTesting.Components.Etsy;

using Domain.Components;
using System.Security.Cryptography;
using System.Text;

/// <summary>Tests</summary>
public class EtsyPkceGeneratorTests
{
    #region <Methods>

    [Fact]
    public void BuildAuthorizeChallenge_ContainsAllRequiredOAuthParameters()
    {
        var challenge = EtsyPkceGenerator.BuildAuthorizeChallenge("clientId123", "https://example.com/callback", "transactions_r shops_r");

        Assert.Contains("response_type=code", challenge.AuthorizeUrl);
        Assert.Contains("client_id=clientId123", challenge.AuthorizeUrl);
        Assert.Contains("redirect_uri=https%3A%2F%2Fexample.com%2Fcallback", challenge.AuthorizeUrl);
        Assert.Contains("scope=transactions_r%20shops_r", challenge.AuthorizeUrl);
        Assert.Contains("code_challenge_method=S256", challenge.AuthorizeUrl);
        Assert.Contains($"state={challenge.State}", challenge.AuthorizeUrl);
        Assert.Contains("prompt=login", challenge.AuthorizeUrl);
    }

    [Fact]
    public void BuildAuthorizeChallenge_StartsWithEtsyAuthorizeUrl()
    {
        var challenge = EtsyPkceGenerator.BuildAuthorizeChallenge("a", "b", "c");

        Assert.StartsWith("https://www.etsy.com/oauth/connect?", challenge.AuthorizeUrl);
    }

    [Fact]
    public void BuildAuthorizeChallenge_CodeVerifierIsBase64UrlSafe()
    {
        var challenge = EtsyPkceGenerator.BuildAuthorizeChallenge("a", "b", "c");

        Assert.DoesNotContain('+', challenge.CodeVerifier);
        Assert.DoesNotContain('/', challenge.CodeVerifier);
        Assert.DoesNotContain('=', challenge.CodeVerifier);
    }

    [Fact]
    public void BuildAuthorizeChallenge_CodeVerifierMeetsPkceLengthRequirement()
    {
        // RFC 7636 requires code_verifier to be 43-128 characters
        var challenge = EtsyPkceGenerator.BuildAuthorizeChallenge("a", "b", "c");

        Assert.InRange(challenge.CodeVerifier.Length, 43, 128);
    }

    [Fact]
    public void BuildAuthorizeChallenge_StateIsNonEmpty()
    {
        var challenge = EtsyPkceGenerator.BuildAuthorizeChallenge("a", "b", "c");

        Assert.False(string.IsNullOrEmpty(challenge.State));
    }

    [Fact]
    public void BuildAuthorizeChallenge_TwoCallsProduceDifferentChallenges()
    {
        var first = EtsyPkceGenerator.BuildAuthorizeChallenge("a", "b", "c");
        var second = EtsyPkceGenerator.BuildAuthorizeChallenge("a", "b", "c");

        Assert.NotEqual(first.CodeVerifier, second.CodeVerifier);
        Assert.NotEqual(first.State, second.State);
    }

    [Fact]
    public void BuildAuthorizeChallenge_CodeChallengeIsSha256OfVerifier()
    {
        // The code_challenge in the URL must equal BASE64URL(SHA256(code_verifier))
        var challenge = EtsyPkceGenerator.BuildAuthorizeChallenge("a", "b", "c");

        var expected = Base64UrlEncode(SHA256.HashData(Encoding.ASCII.GetBytes(challenge.CodeVerifier)));
        Assert.Contains($"code_challenge={Uri.EscapeDataString(expected)}", challenge.AuthorizeUrl);
    }

    [Fact]
    public void BuildAuthorizeChallenge_SpecialCharactersInInputsAreUrlEncoded()
    {
        var challenge = EtsyPkceGenerator.BuildAuthorizeChallenge("client&id", "https://a.com/cb?x=1", "scope with space");

        Assert.Contains("client_id=client%26id", challenge.AuthorizeUrl);
        Assert.Contains("redirect_uri=https%3A%2F%2Fa.com%2Fcb%3Fx%3D1", challenge.AuthorizeUrl);
        Assert.Contains("scope=scope%20with%20space", challenge.AuthorizeUrl);
    }

    [Fact]
    public void ExtractEtsyUserId_PrefixBeforeDot_ReturnsPrefix()
    {
        var result = EtsyPkceGenerator.ExtractEtsyUserId("1226690893.VQ3xh7ilSophBhnv7j6Rx");

        Assert.Equal("1226690893", result);
    }

    [Fact]
    public void ExtractEtsyUserId_NoDot_ReturnsEmpty()
    {
        var result = EtsyPkceGenerator.ExtractEtsyUserId("notoken");

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ExtractEtsyUserId_EmptyString_ReturnsEmpty()
    {
        var result = EtsyPkceGenerator.ExtractEtsyUserId(string.Empty);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ExtractEtsyUserId_DotAtStart_ReturnsEmpty()
    {
        // Leading dot means no prefix — should return empty, not throw
        var result = EtsyPkceGenerator.ExtractEtsyUserId(".abc");

        Assert.Equal(string.Empty, result);
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    #endregion
}
