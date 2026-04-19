namespace My.Talli.Web.Services.Platforms;

using Domain.Models;

/// <summary>Service</summary>
public interface IPlatformSyncService
{
    #region <Properties>

    string Platform { get; }

    #endregion

    #region <Methods>

    Task<PlatformSyncResult> SyncShopAsync(ShopConnection shop, CancellationToken cancellationToken);

    #endregion
}
