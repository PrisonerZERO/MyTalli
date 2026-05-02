namespace My.Talli.Web.Services.Platforms;

using Microsoft.Extensions.Options;
using Stripe;
using Web.Services.Billing;

/// <summary>Service</summary>
public class StripeConnectService : IStripeConnectApiClient
{
    #region <Variables>

    private readonly StripeSettings _billingSettings;
    private readonly ILogger<StripeConnectService> _logger;
    private readonly StripeConnectSettings _settings;

    #endregion

    #region <Constructors>

    public StripeConnectService(ILogger<StripeConnectService> logger, IOptions<StripeConnectSettings> settings, IOptions<StripeSettings> billingSettings)
    {
        _billingSettings = billingSettings.Value;
        _logger = logger;
        _settings = settings.Value;
        StripeConfiguration.ApiKey = _billingSettings.SecretKey;
    }

    #endregion

    #region <Properties>

    public string RefreshUri => _settings.RefreshUri;

    public string ReturnUri => _settings.ReturnUri;

    #endregion

    #region <Methods>

    public async Task<Account> CreateConnectedAccountAsync(CancellationToken cancellationToken)
    {
        var service = new AccountService();
        var options = new AccountCreateOptions { Type = "standard" };
        var account = await service.CreateAsync(options, cancellationToken: cancellationToken);

        _logger.LogInformation("Stripe connected account created: {AccountId}", account.Id);
        return account;
    }

    public async Task<AccountLink> CreateAccountLinkAsync(string accountId, string returnUri, string refreshUri, CancellationToken cancellationToken)
    {
        var service = new AccountLinkService();
        var options = new AccountLinkCreateOptions
        {
            Account = accountId,
            RefreshUrl = refreshUri,
            ReturnUrl = returnUri,
            Type = "account_onboarding"
        };

        var link = await service.CreateAsync(options, cancellationToken: cancellationToken);
        return link;
    }

    public async Task<Account> GetAccountAsync(string accountId, CancellationToken cancellationToken)
    {
        var service = new AccountService();
        return await service.GetAsync(accountId, cancellationToken: cancellationToken);
    }

    public async Task<StripeList<Charge>> ListChargesAsync(string accountId, DateTime? createdAfter, string? startingAfter, int limit, CancellationToken cancellationToken)
    {
        var options = new ChargeListOptions
        {
            Expand = new List<string> { "data.balance_transaction" },
            Limit = limit
        };

        if (createdAfter.HasValue)
            options.Created = new DateRangeOptions { GreaterThan = createdAfter.Value };

        if (!string.IsNullOrEmpty(startingAfter))
            options.StartingAfter = startingAfter;

        var requestOptions = new RequestOptions { StripeAccount = accountId };
        var service = new ChargeService();
        return await service.ListAsync(options, requestOptions, cancellationToken);
    }

    public async Task<StripeList<Payout>> ListPayoutsAsync(string accountId, DateTime? createdAfter, string? startingAfter, int limit, CancellationToken cancellationToken)
    {
        var options = new PayoutListOptions { Limit = limit };

        if (createdAfter.HasValue)
            options.Created = new DateRangeOptions { GreaterThan = createdAfter.Value };

        if (!string.IsNullOrEmpty(startingAfter))
            options.StartingAfter = startingAfter;

        var requestOptions = new RequestOptions { StripeAccount = accountId };
        var service = new PayoutService();
        return await service.ListAsync(options, requestOptions, cancellationToken);
    }

    #endregion
}
