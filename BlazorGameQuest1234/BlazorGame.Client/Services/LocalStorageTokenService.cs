using Microsoft.JSInterop;

namespace BlazorGame.Client.Services;

/// <summary>
/// Service de gestion des tokens dans le LocalStorage
/// </summary>
public class LocalStorageTokenService : ITokenService
{
    private readonly IJSRuntime _jsRuntime;
    private const string TOKEN_KEY = "blazor_auth_token";

    public LocalStorageTokenService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", TOKEN_KEY);
        }
        catch
        {
            return null;
        }
    }

    public async Task SetTokenAsync(string token)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TOKEN_KEY, token);
    }

    public async Task RemoveTokenAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TOKEN_KEY);
    }
}