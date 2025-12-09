using Microsoft.JSInterop;

namespace BlazorGame.Client.Services;

/// <summary>
/// Service de gestion des tokens JWT dans le LocalStorage du navigateur
/// Utilisé pour persister l'authentification Keycloak entre les sessions
/// </summary>
public class LocalStorageTokenService : ITokenService
{
    private readonly IJSRuntime _jsRuntime;
    private const string TOKEN_KEY = "blazor_auth_token";

    public LocalStorageTokenService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Récupère le token JWT depuis le localStorage
    /// </summary>
    /// <returns>Token JWT ou null si absent</returns>
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

    /// <summary>
    /// Enregistre le token JWT dans le localStorage
    /// </summary>
    /// <param name="token">Token JWT à enregistrer</param>
    public async Task SetTokenAsync(string token)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TOKEN_KEY, token);
    }

    /// <summary>
    /// Supprime le token JWT du localStorage (lors de la déconnexion)
    /// </summary>
    public async Task RemoveTokenAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TOKEN_KEY);
    }
}