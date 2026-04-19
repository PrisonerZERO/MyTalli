namespace My.Talli.Web.Services.Platforms;

using Domain.Components;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;

/// <summary>Service</summary>
public class EtsyService
{
    #region <Constants>

    public const string ShopReceiptsUrlTemplate = "https://openapi.etsy.com/v3/application/shops/{0}/receipts";
    public const string TokenUrl = "https://api.etsy.com/v3/public/oauth/token";
    public const string UserShopsUrlTemplate = "https://openapi.etsy.com/v3/application/users/{0}/shops";

    #endregion

    #region <Variables>

    private readonly HttpClient _httpClient;
    private readonly ILogger<EtsyService> _logger;
    private readonly EtsySettings _settings;

    #endregion

    #region <Constructors>

    public EtsyService(HttpClient httpClient, ILogger<EtsyService> logger, IOptions<EtsySettings> settings)
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = settings.Value;
    }

    #endregion

    #region <Methods>

    public AuthorizeChallenge BuildAuthorizeChallenge()
    {
        return EtsyPkceGenerator.BuildAuthorizeChallenge(_settings.ClientId, _settings.RedirectUri, _settings.Scope);
    }

    public async Task<EtsyTokenResponse> ExchangeCodeAsync(string code, string codeVerifier)
    {
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = _settings.ClientId,
            ["redirect_uri"] = _settings.RedirectUri,
            ["code"] = code,
            ["code_verifier"] = codeVerifier
        });

        var response = await _httpClient.PostAsync(TokenUrl, form);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Etsy token exchange failed: {Status} {Body}", response.StatusCode, body);
            throw new InvalidOperationException($"Etsy token exchange failed with status {(int)response.StatusCode}.");
        }

        var payload = await response.Content.ReadFromJsonAsync<EtsyTokenResponse>();
        if (payload is null || string.IsNullOrEmpty(payload.AccessToken))
            throw new InvalidOperationException("Etsy token exchange returned an empty response.");

        return payload;
    }

    public async Task<EtsyTokenResponse> RefreshTokensAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = _settings.ClientId,
            ["refresh_token"] = refreshToken
        });

        var response = await _httpClient.PostAsync(TokenUrl, form, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Etsy token refresh failed: {Status} {Body}", response.StatusCode, body);
            throw new InvalidOperationException($"Etsy token refresh failed with status {(int)response.StatusCode}.");
        }

        var payload = await response.Content.ReadFromJsonAsync<EtsyTokenResponse>(cancellationToken);
        if (payload is null || string.IsNullOrEmpty(payload.AccessToken))
            throw new InvalidOperationException("Etsy token refresh returned an empty response.");

        return payload;
    }

    public async Task<List<EtsyShop>> GetShopsAsync(string etsyUserId, string accessToken)
    {
        var url = string.Format(UserShopsUrlTemplate, etsyUserId);
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add("x-api-key", $"{_settings.ClientId}:{_settings.ClientSecret}");

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Etsy shop fetch failed: {Status} {Body}", response.StatusCode, body);
            return [];
        }

        var shop = await response.Content.ReadFromJsonAsync<EtsyShop>();
        return shop is null || shop.ShopId == 0 ? [] : [shop];
    }

    public async Task<EtsyReceiptsResponse> GetReceiptsAsync(long shopId, string accessToken, long? minCreated, int limit, int offset, CancellationToken cancellationToken)
    {
        var baseUrl = string.Format(ShopReceiptsUrlTemplate, shopId);
        var queryParams = new List<string>
        {
            $"limit={limit}",
            $"offset={offset}",
            "was_paid=true",
            "sort_on=created",
            "sort_order=asc"
        };

        if (minCreated.HasValue)
            queryParams.Add($"min_created={minCreated.Value}");

        var url = $"{baseUrl}?{string.Join("&", queryParams)}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add("x-api-key", $"{_settings.ClientId}:{_settings.ClientSecret}");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Etsy receipts fetch failed for shop {ShopId}: {Status} {Body}", shopId, response.StatusCode, body);
            throw new InvalidOperationException($"Etsy receipts fetch failed with status {(int)response.StatusCode}.");
        }

        var payload = await response.Content.ReadFromJsonAsync<EtsyReceiptsResponse>(cancellationToken);
        return payload ?? new EtsyReceiptsResponse();
    }

    #endregion
}
