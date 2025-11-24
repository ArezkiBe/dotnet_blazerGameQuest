using SharedModels.Models;

namespace GameAPI.Services;

/// <summary>
/// Service pour la gestion des sessions de jeu
/// </summary>
public interface IGameSessionService
{
    /// <summary>
    /// Démarre une nouvelle aventure pour un joueur
    /// </summary>
    /// <param name="playerId">ID du joueur</param>
    /// <param name="difficultyLevel">Niveau de difficulté souhaité</param>
    /// <returns>La session de jeu créée</returns>
    Task<GameSession> StartNewAdventureAsync(int playerId, int difficultyLevel = 2);
    
    /// <summary>
    /// Récupère une session de jeu active
    /// </summary>
    /// <param name="sessionId">ID de la session</param>
    /// <returns>La session avec les détails de la salle actuelle</returns>
    Task<GameSession?> GetActiveSessionAsync(int sessionId);
    
    /// <summary>
    /// Récupère une session de jeu (quel que soit son statut)
    /// </summary>
    /// <param name="sessionId">ID de la session</param>
    /// <returns>La session avec ses détails</returns>
    Task<GameSession?> GetSessionAsync(int sessionId);
    
    /// <summary>
    /// Effectue une action dans la salle actuelle
    /// </summary>
    /// <param name="sessionId">ID de la session</param>
    /// <param name="actionType">Type d'action à effectuer</param>
    /// <returns>Le résultat de l'action</returns>
    Task<GameAction> PerformActionAsync(int sessionId, ActionType actionType);
    
    /// <summary>
    /// Passe à la salle suivante dans le donjon
    /// </summary>
    /// <param name="sessionId">ID de la session</param>
    /// <returns>True si la progression est possible</returns>
    Task<bool> MoveToNextRoomAsync(int sessionId);
    
    /// <summary>
    /// Termine une session de jeu
    /// </summary>
    /// <param name="sessionId">ID de la session</param>
    /// <param name="status">Statut final (Completed ou Failed)</param>
    /// <returns>La session mise à jour</returns>
    Task<GameSession> EndSessionAsync(int sessionId, GameStatus status);
    
    /// <summary>
    /// Récupère la salle actuelle d'une session
    /// </summary>
    /// <param name="sessionId">ID de la session</param>
    /// <returns>Les détails de la salle actuelle</returns>
    Task<Room?> GetCurrentRoomAsync(int sessionId);
    
    /// <summary>
    /// Récupère l'historique des actions d'une session
    /// </summary>
    /// <param name="sessionId">ID de la session</param>
    /// <returns>Liste des actions effectuées</returns>
    Task<List<GameAction>> GetSessionActionsAsync(int sessionId);
}