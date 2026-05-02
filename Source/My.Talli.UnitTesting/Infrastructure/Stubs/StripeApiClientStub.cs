namespace My.Talli.UnitTesting.Infrastructure.Stubs;

using My.Talli.Web.Services.Platforms;
using Stripe;

using STRIPE = Stripe;

/// <summary>Stub</summary>
public class StripeApiClientStub : IStripeConnectApiClient
{
    #region <Properties>

    public List<(string AccountId, DateTime? CreatedAfter, string? StartingAfter)> ChargeCalls { get; } = new();

    public Queue<StripeList<Charge>> ChargeResponses { get; } = new();

    public List<(string AccountId, DateTime? CreatedAfter, string? StartingAfter)> PayoutCalls { get; } = new();

    public Queue<StripeList<STRIPE.Payout>> PayoutResponses { get; } = new();

    #endregion

    #region <Methods>

    public Task<Account> CreateConnectedAccountAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException("CreateConnectedAccountAsync not used by these tests.");
    }

    public Task<AccountLink> CreateAccountLinkAsync(string accountId, string returnUri, string refreshUri, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("CreateAccountLinkAsync not used by these tests.");
    }

    public Task<Account> GetAccountAsync(string accountId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("GetAccountAsync not used by these tests.");
    }

    public Task<StripeList<Charge>> ListChargesAsync(string accountId, DateTime? createdAfter, string? startingAfter, int limit, CancellationToken cancellationToken)
    {
        ChargeCalls.Add((accountId, createdAfter, startingAfter));
        var response = ChargeResponses.Count > 0 ? ChargeResponses.Dequeue() : new StripeList<Charge> { Data = new List<Charge>() };
        return Task.FromResult(response);
    }

    public Task<StripeList<STRIPE.Payout>> ListPayoutsAsync(string accountId, DateTime? createdAfter, string? startingAfter, int limit, CancellationToken cancellationToken)
    {
        PayoutCalls.Add((accountId, createdAfter, startingAfter));
        var response = PayoutResponses.Count > 0 ? PayoutResponses.Dequeue() : new StripeList<STRIPE.Payout> { Data = new List<STRIPE.Payout>() };
        return Task.FromResult(response);
    }

    #endregion
}
