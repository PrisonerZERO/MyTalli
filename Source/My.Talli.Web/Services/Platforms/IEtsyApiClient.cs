namespace My.Talli.Web.Services.Platforms;

using Domain.Components;

/// <summary>Client</summary>
public interface IEtsyApiClient
{
    #region <Methods>

    Task<EtsyTokenResponse> ExchangeCodeAsync(string code, string codeVerifier);

    Task<EtsyLedgerEntriesResponse> GetLedgerEntriesAsync(long shopId, string accessToken, long? minCreated, int limit, int offset, CancellationToken cancellationToken);

    Task<EtsyReceiptsResponse> GetReceiptsAsync(long shopId, string accessToken, long? minCreated, int limit, int offset, CancellationToken cancellationToken);

    Task<List<EtsyShop>> GetShopsAsync(string etsyUserId, string accessToken);

    Task<EtsyTokenResponse> RefreshTokensAsync(string refreshToken, CancellationToken cancellationToken);

    #endregion
}
