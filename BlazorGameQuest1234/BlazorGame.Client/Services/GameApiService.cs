using SharedModels.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace BlazorGame.Client.Services;

/// <summary>
/// Service pour communiquer avec l'API de jeu
/// Gère toutes les requêtes HTTP vers le backend
/// </summary>
public class GameApiService : IGameApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public GameApiService(HttpClient httpClient)
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
                null);
            
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
            var response = await _httpClient.PostAsync($"api/game/session/{sessionId}/next-room", null);
            
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
                null);
            
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

}