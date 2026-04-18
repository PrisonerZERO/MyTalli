namespace My.Talli.Web.Services.Platforms;

using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

/// <summary>Service</summary>
public class EtsyService
{
    #region <Constants>

    public const string AuthorizeUrl = "https://www.etsy.com/oauth/connect";
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
        var codeVerifier = GenerateCodeVerifier();
        var codeChallenge = GenerateCodeChallenge(codeVerifier);
        var state = GenerateState();

        var authorizeUrl =
            $"{AuthorizeUrl}" +
            $"?response_type=code" +
            $"&client_id={Uri.EscapeDataString(_settings.ClientId)}" +
            $"&redirect_uri={Uri.EscapeDataString(_settings.RedirectUri)}" +
            $"&scope={Uri.EscapeDataString(_settings.Scope)}" +
            $"&state={Uri.EscapeDataString(state)}" +
            $"&code_challenge={Uri.EscapeDataString(codeChallenge)}" +
            $"&code_challenge_method=S256";

        return new AuthorizeChallenge
        {
            AuthorizeUrl = authorizeUrl,
            CodeVerifier = codeVerifier,
            State = state
        };
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

    public static string ExtractEtsyUserId(string accessToken)
    {
        if (string.IsNullOrEmpty(accessToken))
            return string.Empty;

        var dot = accessToken.IndexOf('.');
        return dot > 0 ? accessToken[..dot] : string.Empty;
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        var bytes = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
        return Base64UrlEncode(bytes);
    }

    private static string GenerateCodeVerifier()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Base64UrlEncode(bytes);
    }

    private static string GenerateState()
    {
        var bytes = RandomNumberGenerator.GetBytes(16);
        return Base64UrlEncode(bytes);
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    #endregion
}

/// <summary>Challenge</summary>
public class AuthorizeChallenge
{
    #region <Properties>

    public string AuthorizeUrl { get; set; } = string.Empty;

    public string CodeVerifier { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    #endregion
}

/// <summary>Response</summary>
public class EtsyTokenResponse
{
    #region <Properties>

    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    #endregion
}

/// <summary>Model</summary>
public class EtsyShop
{
    #region <Properties>

    [JsonPropertyName("shipping_from_country_iso")]
    public string? CountryIso { get; set; }

    [JsonPropertyName("currency_code")]
    public string? CurrencyCode { get; set; }

    [JsonPropertyName("is_vacation")]
    public bool IsVacation { get; set; }

    [JsonPropertyName("shop_id")]
    public long ShopId { get; set; }

    [JsonPropertyName("shop_name")]
    public string? ShopName { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    #endregion
}