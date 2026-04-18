namespace My.Talli.Web.Services.Platforms;

using Domain.Components;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;

/// <summary>Service</summary>
public class EtsyService
{
    #region <Constants>

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

    #endregion
}
