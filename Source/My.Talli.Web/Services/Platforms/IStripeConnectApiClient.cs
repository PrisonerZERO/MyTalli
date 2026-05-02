namespace My.Talli.Web.Services.Platforms;

using Stripe;

/// <summary>Client</summary>
public interface IStripeConnectApiClient
{
    #region <Methods>

    Task<Account> CreateConnectedAccountAsync(CancellationToken cancellationToken);

    Task<AccountLink> CreateAccountLinkAsync(string accountId, string returnUri, string refreshUri, CancellationToken cancellationToken);

    Task<Account> GetAccountAsync(string accountId, CancellationToken cancellationToken);

    Task<StripeList<Charge>> ListChargesAsync(string accountId, DateTime? createdAfter, string? startingAfter, int limit, CancellationToken cancellationToken);

    Task<StripeList<Payout>> ListPayoutsAsync(string accountId, DateTime? createdAfter, string? startingAfter, int limit, CancellationToken cancellationToken);

    #endregion
}
