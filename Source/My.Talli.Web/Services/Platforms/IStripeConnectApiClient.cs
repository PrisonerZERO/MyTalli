namespace My.Talli.Web.Services.Platforms;

using Domain.Components;
using Stripe;

/// <summary>Client</summary>
public interface IStripeConnectApiClient
{
    #region <Methods>

    Task<StripeTokenResponse> ExchangeCodeAsync(string code, CancellationToken cancellationToken);

    Task<Account> GetAccountAsync(string accessToken, CancellationToken cancellationToken);

    Task<StripeList<Charge>> ListChargesAsync(string accessToken, DateTime? createdAfter, string? startingAfter, int limit, CancellationToken cancellationToken);

    Task<StripeList<Payout>> ListPayoutsAsync(string accessToken, DateTime? createdAfter, string? startingAfter, int limit, CancellationToken cancellationToken);

    #endregion
}
