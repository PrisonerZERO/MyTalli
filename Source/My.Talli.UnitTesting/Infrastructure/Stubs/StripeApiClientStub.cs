namespace My.Talli.UnitTesting.Infrastructure.Stubs;

using Domain.Components;
using My.Talli.Web.Services.Platforms;
using Stripe;

using STRIPE = Stripe;

/// <summary>Stub</summary>
public class StripeApiClientStub : IStripeConnectApiClient
{
    #region <Properties>

    public List<(string AccessToken, DateTime? CreatedAfter, string? StartingAfter)> ChargeCalls { get; } = new();

    public Queue<StripeList<Charge>> ChargeResponses { get; } = new();

    public List<(string AccessToken, DateTime? CreatedAfter, string? StartingAfter)> PayoutCalls { get; } = new();

    public Queue<StripeList<STRIPE.Payout>> PayoutResponses { get; } = new();

    #endregion

    #region <Methods>

    public Task<StripeTokenResponse> ExchangeCodeAsync(string code, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("ExchangeCodeAsync not used by these tests.");
    }

    public Task<Account> GetAccountAsync(string accessToken, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("GetAccountAsync not used by these tests.");
    }

    public Task<StripeList<Charge>> ListChargesAsync(string accessToken, DateTime? createdAfter, string? startingAfter, int limit, CancellationToken cancellationToken)
    {
        ChargeCalls.Add((accessToken, createdAfter, startingAfter));
        var response = ChargeResponses.Count > 0 ? ChargeResponses.Dequeue() : new StripeList<Charge> { Data = new List<Charge>() };
        return Task.FromResult(response);
    }

    public Task<StripeList<STRIPE.Payout>> ListPayoutsAsync(string accessToken, DateTime? createdAfter, string? startingAfter, int limit, CancellationToken cancellationToken)
    {
        PayoutCalls.Add((accessToken, createdAfter, startingAfter));
        var response = PayoutResponses.Count > 0 ? PayoutResponses.Dequeue() : new StripeList<STRIPE.Payout> { Data = new List<STRIPE.Payout>() };
        return Task.FromResult(response);
    }

    #endregion
}
