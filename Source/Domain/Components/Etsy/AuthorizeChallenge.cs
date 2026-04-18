namespace My.Talli.Domain.Components;

/// <summary>Challenge</summary>
public class AuthorizeChallenge
{
    #region <Properties>

    public string AuthorizeUrl { get; set; } = string.Empty;

    public string CodeVerifier { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    #endregion
}
