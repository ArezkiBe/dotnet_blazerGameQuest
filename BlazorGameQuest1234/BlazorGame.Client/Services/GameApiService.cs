using SharedModels.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace BlazorGame.Client.Services;

/// <summary>
/// Service pour communiquer avec l'API de jeu
/// Gère toutes les requêtes HTTP vers le backend avec authentification
/// </summary>
public class GameApiService : IGameApiService
{
    private readonly AuthenticatedHttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public GameApiService(AuthenticatedHttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Initialise une nouvelle session de jeu avec génération automatique du donjon
    /// </summary>
    /// <param name="playerId">ID du joueur</param>
    /// <param name="difficulty">Niveau de difficulté 1-5 (défaut: 2)</param>
    /// <returns>Session créée ou null en cas d'erreur</returns>
    public async Task<GameSession?> StartNewAdventureAsync(int playerId, int difficulty = 2)
    {
        try
        {
            var response = await _httpClient.PostAsync(
                $"api/game/start-adventure?playerId={playerId}&difficulty={difficulty}", 
                new StringContent(""));
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<GameSession>(_jsonOptions);
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors du démarrage de l'aventure: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Récupère l'état complet d'une session avec salle actuelle et statistiques
    /// </summary>
    /// <param name="sessionId">Identifiant de la session</param>
    /// <returns>Détails complets de la session ou null si introuvable</returns>
    public async Task<GameSessionDetails?> GetGameSessionAsync(int sessionId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/game/session/{sessionId}");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<GameSessionDetails>(_jsonOptions);
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la récupération de la session: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Exécute une action de jeu avec calcul des conséquences (HP, XP, score)
    /// </summary>
    /// <param name="sessionId">Identifiant de la session</param>
    /// <param name="actionType">Type d'action à effectuer</param>
    /// <returns>Résultat de l'action ou null en cas d'erreur</returns>
    public async Task<GameAction?> PerformActionAsync(int sessionId, ActionType actionType)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"api/game/session/{sessionId}/action", 
                actionType, 
                _jsonOptions);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<GameAction>(_jsonOptions);
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'action: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Passe à la salle suivante
    /// </summary>
    public async Task<RoomProgressionResult?> MoveToNextRoomAsync(int sessionId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/game/session/{sessionId}/next-room", new StringContent(""));
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<RoomProgressionResult>(_jsonOptions);
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la progression: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Termine une session
    /// </summary>
    public async Task<GameSession?> EndSessionAsync(int sessionId, string reason = "Abandon")
    {
        try
        {
            var response = await _httpClient.PostAsync(
                $"api/game/session/{sessionId}/end?reason={reason}", 
                new StringContent(""));
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<GameSession>(_jsonOptions);
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la fin de session: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Récupère la liste des joueurs
    /// </summary>
    public async Task<List<Player>> GetPlayersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/players");
            
            if (response.IsSuccessStatusCode)
            {
                var players = await response.Content.ReadFromJsonAsync<List<Player>>(_jsonOptions);
                return players ?? new List<Player>();
            }
            
            return new List<Player>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la récupération des joueurs: {ex.Message}");
            return new List<Player>();
        }
    }

    /// <summary>
    /// Récupère tous les utilisateurs
    /// </summary>
    /// <returns>Liste des utilisateurs</returns>
    public async Task<List<User>> GetUsersAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<User>>("api/users", _jsonOptions);
            return response ?? new List<User>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la récupération des utilisateurs: {ex.Message}");
            return new List<User>();
        }
    }

    /// <summary>
    /// Récupère un utilisateur par son ID
    /// </summary>
    /// <param name="userId">ID de l'utilisateur</param>
    /// <returns>L'utilisateur ou null si non trouvé</returns>
    public async Task<User?> GetUserByIdAsync(int userId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<User>($"api/users/{userId}", _jsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la récupération de l'utilisateur {userId}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Récupère un utilisateur par son nom d'utilisateur
    /// </summary>
    /// <param name="username">Nom d'utilisateur</param>
    /// <returns>L'utilisateur ou null si non trouvé</returns>
    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<User>($"api/users/by-username/{username}", _jsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la récupération de l'utilisateur {username}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Récupère l'historique des sessions de jeu d'un utilisateur
    /// </summary>
    /// <param name="userId">ID de l'utilisateur</param>
    /// <returns>Liste des sessions de jeu</returns>
    public async Task<List<GameSession>> GetUserGameSessionsAsync(int userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/players/user/{userId}/sessions");
            
            if (response.IsSuccessStatusCode)
            {
                var sessions = await response.Content.ReadFromJsonAsync<List<GameSession>>(_jsonOptions);
                return sessions ?? new List<GameSession>();
            }
            
            return new List<GameSession>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la récupération des sessions de l'utilisateur {userId}: {ex.Message}");
            return new List<GameSession>();
        }
    }

    /// <summary>
    /// Récupère les informations publiques des utilisateurs
    /// </summary>
    public async Task<List<User>> GetPublicUsersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/users/public");
            
            if (response.IsSuccessStatusCode)
            {
                var users = await response.Content.ReadFromJsonAsync<List<User>>(_jsonOptions);
                return users ?? new List<User>();
            }
            
            return new List<User>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la récupération des utilisateurs publics: {ex.Message}");
            return new List<User>();
        }
    }

    /// <summary>
    /// Récupère les données du joueur actuel par nom d'utilisateur
    /// </summary>
    public async Task<(Player? Player, List<GameSession> Sessions)> GetPlayerByUsernameAsync(string username)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/players/by-username/{username}");
            
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<JsonElement>(jsonContent);
                
                Player? player = null;
                var sessions = new List<GameSession>();
                
                if (data.TryGetProperty("player", out var playerElement))
                {
                    player = JsonSerializer.Deserialize<Player>(playerElement.GetRawText(), _jsonOptions);
                }
                
                if (data.TryGetProperty("sessions", out var sessionsElement))
                {
                    sessions = JsonSerializer.Deserialize<List<GameSession>>(sessionsElement.GetRawText(), _jsonOptions) ?? new List<GameSession>();
                }
                
                return (player, sessions);
            }
            
            return (null, new List<GameSession>());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la récupération des données du joueur {username}: {ex.Message}");
            return (null, new List<GameSession>());
        }
    }

    /// <summary>
    /// Met à jour le statut d'activation d'un utilisateur (ADMIN SEULEMENT)
    /// </summary>
    /// <param name="userId">ID de l'utilisateur</param>
    /// <param name="isActive">Nouveau statut d'activation</param>
    /// <returns>True si la mise à jour a réussi</returns>
    public async Task<bool> UpdateUserStatusAsync(int userId, bool isActive)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/users/{userId}/status", isActive, _jsonOptions);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la mise à jour du statut utilisateur {userId}: {ex.Message}");
            return false;
        }
    }
}