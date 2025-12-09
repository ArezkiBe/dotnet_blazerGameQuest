using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace MySecureApi;

public class KeycloakAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    private readonly ILogger<KeycloakAuthService> _logger;

    public KeycloakAuthService(HttpClient httpClient, IConfiguration configuration, ILogger<KeycloakAuthService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<TokenResponse> LoginAsync(string username, string password)
    {
        var tokenEndpoint = $"{_configuration["Keycloak:BaseUrl"]}/realms/{_configuration["Keycloak:Realm"]}/protocol/openid-connect/token";

        #pragma warning disable CS8604 // Possible null reference argument.
        
        var requestBody = new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "client_id", _configuration["Keycloak:ClientId"] },
            { "client_secret", _configuration["Keycloak:ClientSecret"] },
            { "username", username },
            { "password", password }
        };

#pragma warning restore CS8604 // Possible null reference argument.

        var content = new FormUrlEncodedContent(requestBody);

        var response = await _httpClient.PostAsync(tokenEndpoint, content);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResult = JsonSerializer.Deserialize<TokenResponse>(responseContent);
            _logger.LogInformation("Token retrieved successfully");
            return tokenResult!;
        }

        throw new Exception("failed");
    }
}

public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }
}
