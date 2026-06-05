namespace My.Talli.Domain.Commands.Platforms;

/// <summary>Result</summary>
public class SwapShopAccountResult
{
    #region <Properties>

    /// <summary>True when the targeted shop existed and was owned by the user.</summary>
    public bool Found { get; set; }

    /// <summary>True when the user authorized a different account, so the shop was re-pointed and its old data wiped. False when the same account was re-authorized (token refresh only).</summary>
    public bool Swapped { get; set; }

    #endregion
}
