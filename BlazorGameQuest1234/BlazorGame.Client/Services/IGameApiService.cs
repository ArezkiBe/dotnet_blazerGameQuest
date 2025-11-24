using SharedModels.Models;

namespace BlazorGame.Client.Services;

/// <summary>
/// Service pour communiquer avec l'API de jeu
/// </summary>
public interface IGameApiService
{
    /// <summary>
    /// Démarre une nouvelle aventure pour un joueur
    /// </summary>
    /// <param name="playerId">ID du joueur</param>
    /// <param name="difficulty">Niveau de difficulté (1-5)</param>
    /// <returns>La session de jeu créée</returns>
    Task<GameSession?> StartNewAdventureAsync(int playerId, int difficulty = 2);
    
    /// <summary>
    /// Récupère les détails d'une session de jeu
    /// </summary>
    /// <param name="sessionId">ID de la session</param>
    /// <returns>Les détails de la session avec la salle actuelle</returns>
    Task<GameSessionDetails?> GetGameSessionAsync(int sessionId);
    
    /// <summary>
    /// Effectue une action dans la salle actuelle
    /// </summary>
    /// <param name="sessionId">ID de la session</param>
    /// <param name="actionType">Type d'action à effectuer</param>
    /// <returns>Le résultat de l'action</returns>
    Task<GameAction?> PerformActionAsync(int sessionId, ActionType actionType);
    
    /// <summary>
    /// Passe à la salle suivante du donjon
    /// </summary>
    /// <param name="sessionId">ID de la session</param>
    /// <returns>Résultat de la progression</returns>
    Task<RoomProgressionResult?> MoveToNextRoomAsync(int sessionId);
    
    /// <summary>
    /// Termine une session de jeu
    /// </summary>
    /// <param name="sessionId">ID de la session</param>
    /// <param name="reason">Raison de l'arrêt</param>
    /// <returns>La session terminée</returns>
    Task<GameSession?> EndSessionAsync(int sessionId, string reason = "Abandon");
    
    /// <summary>
    /// Récupère la liste des joueurs (pour sélection)
    /// </summary>
    /// <returns>Liste des joueurs disponibles</returns>
    Task<List<Player>> GetPlayersAsync();
    
}