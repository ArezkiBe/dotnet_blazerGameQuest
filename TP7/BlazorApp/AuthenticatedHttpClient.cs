using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace BlazorApp
{
    public class AuthenticatedHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly ITokenService _tokenService;

        public AuthenticatedHttpClient(HttpClient httpClient, ITokenService tokenService)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
        }

        public async Task<T?> GetFromJsonAsync<T>(string requestUri)
        {
            await SetAuthHeaderAsync();
            return await _httpClient.GetFromJsonAsync<T>(requestUri);
        }

        private async Task SetAuthHeaderAsync()
        {
            var token = await _tokenService.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}