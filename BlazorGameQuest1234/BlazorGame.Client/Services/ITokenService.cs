namespace BlazorGame.Client.Services;

/// <summary>
/// Interface pour la gestion des tokens JWT
/// </summary>
public interface ITokenService
{
    Task<string?> GetTokenAsync();
    Task SetTokenAsync(string token);
    Task RemoveTokenAsync();
}