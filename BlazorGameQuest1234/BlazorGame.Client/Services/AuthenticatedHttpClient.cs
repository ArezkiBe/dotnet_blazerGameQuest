using System.Net.Http.Json;

namespace BlazorGame.Client.Services;

/// <summary>
/// HttpClient qui ajoute automatiquement le token JWT aux requêtes
/// </summary>
public class AuthenticatedHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ITokenService _tokenService;

    public AuthenticatedHttpClient(HttpClient httpClient, ITokenService tokenService)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
    }

    public async Task<HttpResponseMessage> GetAsync(string requestUri)
    {
        await SetAuthorizationHeaderAsync();
        return await _httpClient.GetAsync(requestUri);
    }

    public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
    {
        await SetAuthorizationHeaderAsync();
        return await _httpClient.PostAsync(requestUri, content);
    }

    public async Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content)
    {
        await SetAuthorizationHeaderAsync();
        return await _httpClient.PutAsync(requestUri, content);
    }

    public async Task<HttpResponseMessage> DeleteAsync(string requestUri)
    {
        await SetAuthorizationHeaderAsync();
        return await _httpClient.DeleteAsync(requestUri);
    }

    // Méthodes d'extension JSON
    public async Task<HttpResponseMessage> PostAsJsonAsync<T>(string requestUri, T value, System.Text.Json.JsonSerializerOptions? options = null)
    {
        await SetAuthorizationHeaderAsync();
        return await _httpClient.PostAsJsonAsync(requestUri, value, options);
    }

    public async Task<HttpResponseMessage> PutAsJsonAsync<T>(string requestUri, T value, System.Text.Json.JsonSerializerOptions? options = null)
    {
        await SetAuthorizationHeaderAsync();
        return await _httpClient.PutAsJsonAsync(requestUri, value, options);
    }

    public async Task<T?> GetFromJsonAsync<T>(string requestUri, System.Text.Json.JsonSerializerOptions? options = null)
    {
        await SetAuthorizationHeaderAsync();
        return await _httpClient.GetFromJsonAsync<T>(requestUri, options);
    }

    private async Task SetAuthorizationHeaderAsync()
    {
        var token = await _tokenService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }
}