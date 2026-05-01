namespace My.Talli.Web.Services.Platforms;

using Domain.Components;

/// <summary>Client</summary>
public interface IGumroadApiClient
{
    #region <Methods>

    Task<GumroadTokenResponse> ExchangeCodeAsync(string code, CancellationToken cancellationToken);

    Task<GumroadSalesResponse> GetSalesAsync(string accessToken, DateTime? after, string? pageKey, CancellationToken cancellationToken);

    Task<GumroadUser?> GetUserAsync(string accessToken, CancellationToken cancellationToken);

    #endregion
}
