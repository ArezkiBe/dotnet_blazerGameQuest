using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace BlazorGame.Client.Services;

/// <summary>
/// Provider d'authentification personnalisé pour Keycloak
/// Basé sur le TP7, adapté pour BlazorGameQuest
/// </summary>
public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ITokenService _tokenService;
    private readonly ILogger<CustomAuthStateProvider> _logger;
    private readonly IJSRuntime _jsRuntime;

    public CustomAuthStateProvider(ITokenService tokenService, ILogger<CustomAuthStateProvider> logger, IJSRuntime jsRuntime)
    {
        _tokenService = tokenService;
        _logger = logger;
        _jsRuntime = jsRuntime;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _tokenService.GetTokenAsync();

        if (string.IsNullOrEmpty(token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        try
        {
            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            _logger.LogInformation("Utilisateur authentifié: {Username}", 
                user.FindFirst("preferred_username")?.Value ?? "Inconnu");

            return new AuthenticationState(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du parsing du token JWT");
            await _tokenService.RemoveTokenAsync();
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = Convert.FromBase64String(PadBase64(payload));
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        var claims = new List<Claim>();
        
        if (keyValuePairs != null)
        {
            foreach (var kvp in keyValuePairs)
            {
                if (kvp.Key == "realm_access")
                {
                    // Extraire les rôles Keycloak
                    if (kvp.Value is JsonElement realmAccess && 
                        realmAccess.TryGetProperty("roles", out var roles))
                    {
                        foreach (var role in roles.EnumerateArray())
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role.GetString() ?? ""));
                        }
                    }
                }
                else
                {
                    claims.Add(new Claim(kvp.Key, kvp.Value?.ToString() ?? ""));
                }
            }
        }

        return claims;
    }

    private string PadBase64(string base64)
    {
        return base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
    }

    public void NotifyUserAuthentication(string token)
    {
        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        var authState = Task.FromResult(new AuthenticationState(user));
        NotifyAuthenticationStateChanged(authState);
    }

    public async Task Logout()
    {
        try
        {
            // Supprimer le token local
            await _tokenService.RemoveTokenAsync();
            
            // Rediriger vers l'endpoint de déconnexion Keycloak qui va nettoyer la session
            var logoutUrl = "http://localhost:8180/realms/blazor-gamequest/protocol/openid-connect/logout" +
                "?client_id=blazor-client" +
                "&post_logout_redirect_uri=" + Uri.EscapeDataString("http://localhost:5000/login");
            
            // Redirection directe pour s'assurer que la session Keycloak est bien nettoyée
            await _jsRuntime.InvokeVoidAsync("eval", $"window.location.href = '{logoutUrl}'");
            
            _logger.LogInformation("Redirection vers déconnexion Keycloak effectuée");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la déconnexion Keycloak");
            await _tokenService.RemoveTokenAsync(); // Au moins supprimer le token local
            
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));
            NotifyAuthenticationStateChanged(authState);
        }
    }
}