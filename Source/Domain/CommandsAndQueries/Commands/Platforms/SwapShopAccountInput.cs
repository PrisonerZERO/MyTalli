namespace My.Talli.Domain.Commands.Platforms;

/// <summary>Input</summary>
public class SwapShopAccountInput
{
    #region <Properties>

    public string AccessToken { get; set; } = string.Empty;

    /// <summary>Etsy-only shop detail (country / currency / URL / vacation). Null for non-Etsy platforms.</summary>
    public SwapShopEtsyDetail? EtsyDetail { get; set; }

    public string Platform { get; set; } = string.Empty;

    public string PlatformAccountId { get; set; } = string.Empty;

    public string PlatformShopId { get; set; } = string.Empty;

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryDateTime { get; set; }

    public long ShopConnectionId { get; set; }

    public string ShopName { get; set; } = string.Empty;

    public DateTime? TokenExpiryDateTime { get; set; }

    public long UserId { get; set; }

    #endregion
}
