namespace My.Talli.UnitTesting.Infrastructure.Stubs;

using Domain.Components;
using My.Talli.Web.Services.Platforms;

/// <summary>Stub</summary>
public class GumroadApiClientStub : IGumroadApiClient
{
    #region <Properties>

    public List<(DateTime? After, string? PageKey)> SalesCalls { get; } = new();

    public Queue<GumroadSalesResponse> SalesResponses { get; } = new();

    public List<string> UserCalls { get; } = new();

    public GumroadUser? UserResponse { get; set; }

    #endregion

    #region <Methods>

    public Task<GumroadTokenResponse> ExchangeCodeAsync(string code, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("ExchangeCodeAsync not used by these tests.");
    }

    public Task<GumroadSalesResponse> GetSalesAsync(string accessToken, DateTime? after, string? pageKey, CancellationToken cancellationToken)
    {
        SalesCalls.Add((after, pageKey));
        var response = SalesResponses.Count > 0 ? SalesResponses.Dequeue() : new GumroadSalesResponse();
        return Task.FromResult(response);
    }

    public Task<GumroadUser?> GetUserAsync(string accessToken, CancellationToken cancellationToken)
    {
        UserCalls.Add(accessToken);
        return Task.FromResult(UserResponse);
    }

    #endregion
}
