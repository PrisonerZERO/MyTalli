namespace My.Talli.UnitTesting.Infrastructure.Stubs;

using Domain.Components;
using My.Talli.Web.Services.Platforms;

/// <summary>Stub</summary>
public class EtsyApiClientStub : IEtsyApiClient
{
    #region <Properties>

    public Queue<EtsyLedgerEntriesResponse> LedgerResponses { get; } = new();

    public List<(long ShopId, long? MinCreated, int Limit, int Offset)> LedgerCalls { get; } = new();

    public Queue<EtsyReceiptsResponse> ReceiptResponses { get; } = new();

    public List<(long ShopId, long? MinCreated, int Limit, int Offset)> ReceiptCalls { get; } = new();

    public List<string> RefreshCalls { get; } = new();

    public EtsyTokenResponse? RefreshResponse { get; set; }

    #endregion

    #region <Methods>

    public Task<EtsyTokenResponse> ExchangeCodeAsync(string code, string codeVerifier)
    {
        throw new NotImplementedException("ExchangeCodeAsync not used by these tests.");
    }

    public Task<EtsyLedgerEntriesResponse> GetLedgerEntriesAsync(long shopId, string accessToken, long? minCreated, int limit, int offset, CancellationToken cancellationToken)
    {
        LedgerCalls.Add((shopId, minCreated, limit, offset));
        var response = LedgerResponses.Count > 0 ? LedgerResponses.Dequeue() : new EtsyLedgerEntriesResponse();
        return Task.FromResult(response);
    }

    public Task<EtsyReceiptsResponse> GetReceiptsAsync(long shopId, string accessToken, long? minCreated, int limit, int offset, CancellationToken cancellationToken)
    {
        ReceiptCalls.Add((shopId, minCreated, limit, offset));
        var response = ReceiptResponses.Count > 0 ? ReceiptResponses.Dequeue() : new EtsyReceiptsResponse();
        return Task.FromResult(response);
    }

    public Task<List<EtsyShop>> GetShopsAsync(string etsyUserId, string accessToken)
    {
        throw new NotImplementedException("GetShopsAsync not used by these tests.");
    }

    public Task<EtsyTokenResponse> RefreshTokensAsync(string refreshToken, CancellationToken cancellationToken)
    {
        RefreshCalls.Add(refreshToken);
        if (RefreshResponse is null)
            throw new InvalidOperationException("RefreshResponse not configured on stub.");
        return Task.FromResult(RefreshResponse);
    }

    #endregion
}
