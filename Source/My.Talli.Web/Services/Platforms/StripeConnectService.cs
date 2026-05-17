namespace My.Talli.Web.Services.Platforms;

using Domain.Components;
using Microsoft.Extensions.Options;
using Stripe;
using System.Net.Http.Json;
using Web.Services.Billing;

/// <summary>Service</summary>
public class StripeConnectService : IStripeConnectApiClient
{
    #region <Constants>

    public const string TokenUrl = "https://connect.stripe.com/oauth/token";

    #endregion

    #region <Variables>

    private readonly StripeSettings _billingSettings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<StripeConnectService> _logger;
    private readonly StripeConnectSettings _settings;

    #endregion

    #region <Constructors>

    public StripeConnectService(HttpClient httpClient, ILogger<StripeConnectService> logger, IOptions<StripeConnectSettings> settings, IOptions<StripeSettings> billingSettings)
    {
        _billingSettings = billingSettings.Value;
        _httpClient = httpClient;
        _logger = logger;
        _settings = settings.Value;
        StripeConfiguration.ApiKey = _billingSettings.SecretKey;
    }

    #endregion

    #region <Methods>

    public AuthorizeChallenge BuildAuthorizeChallenge()
    {
        return StripeOAuthGenerator.BuildAuthorizeChallenge(_settings.ClientId, _settings.RedirectUri, _settings.Scope);
    }

    public async Task<StripeTokenResponse> ExchangeCodeAsync(string code, CancellationToken cancellationToken)
    {
        // Stripe Connect OAuth: the platform's SecretKey doubles as the OAuth client_secret.
        // No separate Connect secret to manage.
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_secret"] = _billingSettings.SecretKey,
            ["code"] = code,
            ["grant_type"] = "authorization_code"
        });

        var response = await _httpClient.PostAsync(TokenUrl, form, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Stripe Connect token exchange failed: {Status} {Body}", response.StatusCode, body);
            throw new InvalidOperationException($"Stripe Connect token exchange failed with status {(int)response.StatusCode}.");
        }

        var payload = await response.Content.ReadFromJsonAsync<StripeTokenResponse>(cancellationToken);
        if (payload is null || string.IsNullOrEmpty(payload.AccessToken) || string.IsNullOrEmpty(payload.StripeUserId))
            throw new InvalidOperationException("Stripe Connect token exchange returned an empty or incomplete response.");

        return payload;
    }

    public async Task<Account> GetAccountAsync(string accessToken, CancellationToken cancellationToken)
    {
        // GetAsync(null, ...) hits GET /v1/account, which Stripe resolves to "the account this API key belongs to".
        // The OAuth access_token is scoped to the connected account, so this returns the seller's account.
        var service = new AccountService();
        var requestOptions = new RequestOptions { ApiKey = accessToken };
        return await service.GetAsync(id: null, options: null, requestOptions: requestOptions, cancellationToken: cancellationToken);
    }

    public async Task<StripeList<Charge>> ListChargesAsync(string accessToken, DateTime? createdAfter, string? startingAfter, int limit, CancellationToken cancellationToken)
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

        var requestOptions = new RequestOptions { ApiKey = accessToken };
        var service = new ChargeService();
        return await service.ListAsync(options, requestOptions, cancellationToken);
    }

    public async Task<StripeList<Payout>> ListPayoutsAsync(string accessToken, DateTime? createdAfter, string? startingAfter, int limit, CancellationToken cancellationToken)
    {
        var options = new PayoutListOptions { Limit = limit };

        if (createdAfter.HasValue)
            options.Created = new DateRangeOptions { GreaterThan = createdAfter.Value };

        if (!string.IsNullOrEmpty(startingAfter))
            options.StartingAfter = startingAfter;

        var requestOptions = new RequestOptions { ApiKey = accessToken };
        var service = new PayoutService();
        return await service.ListAsync(options, requestOptions, cancellationToken);
    }

    #endregion
}
