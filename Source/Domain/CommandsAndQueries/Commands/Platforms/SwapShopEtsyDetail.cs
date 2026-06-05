namespace My.Talli.Domain.Commands.Platforms;

/// <summary>Detail</summary>
public class SwapShopEtsyDetail
{
    #region <Properties>

    public string CountryCode { get; set; } = string.Empty;

    public bool IsVacationMode { get; set; }

    public string ShopCurrency { get; set; } = string.Empty;

    public string ShopUrl { get; set; } = string.Empty;

    #endregion
}
