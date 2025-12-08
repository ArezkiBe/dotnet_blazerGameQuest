using SharedModels.Models;

namespace BlazorGame.Client.Services;

/// <summary>
/// Service temporaire pour gérer le contexte utilisateur sans authentification
/// Sera remplacé par le système d'authentification en version 5
/// </summary>
public class UserContextService
{
    private User? _currentUser;
    private readonly IGameApiService _gameApiService;

    public UserContextService(IGameApiService gameApiService)
    {
        _gameApiService = gameApiService;
    }

    /// <summary>
    /// Utilisateur actuellement sélectionné
    /// </summary>
    public User? CurrentUser => _currentUser;

    /// <summary>
    /// Indique si l'utilisateur actuel est un administrateur
    /// </summary>
    public bool IsAdmin => _currentUser?.Role == UserRole.Administrator;

    /// <summary>
    /// Indique si l'utilisateur actuel est un joueur
    /// </summary>
    public bool IsPlayer => _currentUser?.Role == UserRole.Player;

    /// <summary>
    /// Indique si un utilisateur est connecté
    /// </summary>
    public bool IsLoggedIn => _currentUser != null;

    /// <summary>
    /// Événement déclenché lorsque l'utilisateur change
    /// </summary>
    public event Action? OnUserChanged;

    /// <summary>
    /// Sélectionne un utilisateur par son ID
    /// </summary>
    /// <param name="userId">ID de l'utilisateur à sélectionner</param>
    public async Task<bool> SelectUserAsync(int userId)
    {
        try
        {
            var user = await _gameApiService.GetUserByIdAsync(userId);
            if (user != null)
            {
                _currentUser = user;
                OnUserChanged?.Invoke();
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Déconnecte l'utilisateur actuel
    /// </summary>
    public void Logout()
    {
        _currentUser = null;
        OnUserChanged?.Invoke();
    }

    /// <summary>
    /// Récupère le profil joueur de l'utilisateur actuel (si c'est un joueur)
    /// </summary>
    public async Task<Player?> GetCurrentPlayerAsync()
    {
        if (_currentUser?.Role != UserRole.Player)
            return null;

        try
        {
            var players = await _gameApiService.GetPlayersAsync();
            return players?.FirstOrDefault(p => p.UserId == _currentUser.Id);
        }
        catch
        {
            return null;
        }
    }
}