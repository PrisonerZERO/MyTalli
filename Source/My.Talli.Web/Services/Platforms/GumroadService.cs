namespace My.Talli.Web.Services.Platforms;

using Domain.Components;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;

/// <summary>Service</summary>
public class GumroadService : IGumroadApiClient
{
    #region <Constants>

    public const string SalesUrl = "https://api.gumroad.com/v2/sales";
    public const string TokenUrl = "https://api.gumroad.com/oauth/token";
    public const string UserUrl = "https://api.gumroad.com/v2/user";

    #endregion

    #region <Variables>

    private readonly HttpClient _httpClient;
    private readonly ILogger<GumroadService> _logger;
    private readonly GumroadSettings _settings;

    #endregion

    #region <Constructors>

    public GumroadService(HttpClient httpClient, ILogger<GumroadService> logger, IOptions<GumroadSettings> settings)
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = settings.Value;
    }

    #endregion

    #region <Methods>

    public AuthorizeChallenge BuildAuthorizeChallenge()
    {
        return GumroadOAuthGenerator.BuildAuthorizeChallenge(_settings.ClientId, _settings.RedirectUri, _settings.Scope);
    }

    public async Task<GumroadTokenResponse> ExchangeCodeAsync(string code, CancellationToken cancellationToken)
    {
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = _settings.ClientId,
            ["client_secret"] = _settings.ClientSecret,
            ["code"] = code,
            ["grant_type"] = "authorization_code",
            ["redirect_uri"] = _settings.RedirectUri
        });

        var response = await _httpClient.PostAsync(TokenUrl, form, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Gumroad token exchange failed: {Status} {Body}", response.StatusCode, body);
            throw new InvalidOperationException($"Gumroad token exchange failed with status {(int)response.StatusCode}.");
        }

        var payload = await response.Content.ReadFromJsonAsync<GumroadTokenResponse>(cancellationToken);
        if (payload is null || string.IsNullOrEmpty(payload.AccessToken))
            throw new InvalidOperationException("Gumroad token exchange returned an empty response.");

        return payload;
    }

    public async Task<GumroadUser?> GetUserAsync(string accessToken, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, UserUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Gumroad user fetch failed: {Status} {Body}", response.StatusCode, body);
            return null;
        }

        var payload = await response.Content.ReadFromJsonAsync<GumroadUserResponse>(cancellationToken);
        return payload?.User;
    }

    public async Task<GumroadSalesResponse> GetSalesAsync(string accessToken, DateTime? after, string? pageKey, CancellationToken cancellationToken)
    {
        var queryParams = new List<string>();

        if (after.HasValue)
            queryParams.Add($"after={after.Value.ToUniversalTime():yyyy-MM-dd}");

        if (!string.IsNullOrEmpty(pageKey))
            queryParams.Add($"page_key={Uri.EscapeDataString(pageKey)}");

        var url = queryParams.Count == 0 ? SalesUrl : $"{SalesUrl}?{string.Join("&", queryParams)}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Gumroad sales fetch failed: {Status} {Body}", response.StatusCode, body);
            throw new InvalidOperationException($"Gumroad sales fetch failed with status {(int)response.StatusCode}.");
        }

        var payload = await response.Content.ReadFromJsonAsync<GumroadSalesResponse>(cancellationToken);
        return payload ?? new GumroadSalesResponse();
    }

    #endregion
}
