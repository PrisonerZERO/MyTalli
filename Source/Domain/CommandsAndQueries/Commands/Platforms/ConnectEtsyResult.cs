namespace My.Talli.Domain.Commands.Platforms;

/// <summary>Result</summary>
public class ConnectEtsyResult
{
    #region <Properties>

    public bool IsFirstConnection { get; set; }

    public int NewShopCount { get; set; }

    public int RefreshedShopCount { get; set; }

    #endregion
}
