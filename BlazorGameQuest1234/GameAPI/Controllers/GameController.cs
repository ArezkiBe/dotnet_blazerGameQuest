using Microsoft.AspNetCore.Mvc;
using SharedModels.Models;
using GameAPI.Services;

namespace GameAPI.Controllers;

/// <summary>
/// API Controller pour la gestion des sessions de jeu
/// Gère le cycle complet d'une partie : nouvelle aventure, actions, progression
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Game")]
public class GameController : ControllerBase
{
    private readonly IGameSessionService _gameSessionService;
    private readonly ILogger<GameController> _logger;

    public GameController(IGameSessionService gameSessionService, ILogger<GameController> logger)
    {
        _gameSessionService = gameSessionService;
        _logger = logger;
    }

    /// <summary>
    /// Démarre une nouvelle aventure pour un joueur
    /// </summary>
    /// <param name="playerId">ID du joueur</param>
    /// <param name="difficulty">Niveau de difficulté (1-5, défaut: 2)</param>
    /// <returns>La session de jeu créée</returns>
    /// <response code="200">Nouvelle aventure démarrée</response>
    /// <response code="404">Joueur non trouvé</response>
    [HttpPost("start-adventure")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GameSession>> StartNewAdventure([FromQuery] int playerId, [FromQuery] int difficulty = 2)
    {
        try
        {
            var session = await _gameSessionService.StartNewAdventureAsync(playerId, difficulty);
            _logger.LogInformation("Nouvelle aventure démarrée pour le joueur {PlayerId}, session {SessionId}", playerId, session.Id);
            return Ok(session);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Tentative de démarrage d'aventure avec joueur invalide: {PlayerId}", playerId);
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Récupère les détails d'une session de jeu active
    /// </summary>
    /// <param name="sessionId">ID de la session</param>
    /// <returns>Les détails de la session avec la salle actuelle</returns>
    [HttpGet("session/{sessionId}")]
    public async Task<ActionResult<GameSessionDetails>> GetGameSession(int sessionId)
    {
        var session = await _gameSessionService.GetActiveSessionAsync(sessionId);
        if (session == null)
        {
            return NotFound("Session de jeu non trouvée ou inactive");
        }

        var currentRoom = await _gameSessionService.GetCurrentRoomAsync(sessionId);
        var actions = await _gameSessionService.GetSessionActionsAsync(sessionId);

        var result = new GameSessionDetails
        {
            Session = session,
            CurrentRoom = currentRoom,
            ActionHistory = actions.TakeLast(5).ToList(),
            AvailableActions = GetAvailableActions(currentRoom)
        };

        return Ok(result);
    }

    /// <summary>
    /// Effectue une action dans la salle actuelle
    /// </summary>
    /// <param name="sessionId">ID de la session</param>
    /// <param name="actionType">Type d'action à effectuer</param>
    /// <returns>Le résultat de l'action</returns>
    [HttpPost("session/{sessionId}/action")]
    public async Task<ActionResult<GameAction>> PerformAction(int sessionId, [FromBody] ActionType actionType)
    {
        try
        {
            var result = await _gameSessionService.PerformActionAsync(sessionId, actionType);
            _logger.LogInformation("Action {ActionType} effectuée dans la session {SessionId}", actionType, sessionId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Passe à la salle suivante du donjon
    /// </summary>
    /// <param name="sessionId">ID de la session</param>
    /// <returns>Confirmation de la progression</returns>
    [HttpPost("session/{sessionId}/next-room")]
    public async Task<ActionResult<RoomProgressionResult>> MoveToNextRoom(int sessionId)
    {
        var success = await _gameSessionService.MoveToNextRoomAsync(sessionId);
        if (!success)
        {
            return BadRequest("Impossible de progresser - session inactive ou donjon terminé");
        }

        // Récupérer la session (active ou terminée) après la progression
        var session = await _gameSessionService.GetActiveSessionAsync(sessionId) 
                      ?? await _gameSessionService.GetSessionAsync(sessionId);
        var currentRoom = await _gameSessionService.GetCurrentRoomAsync(sessionId);

        var result = new RoomProgressionResult
        {
            Message = session?.Status == GameStatus.Completed 
                ? "🎉 Félicitations ! Vous avez terminé le donjon !" 
                : $"🚪 Vous entrez dans la salle {session?.CurrentRoomNumber}",
            Session = session,
            CurrentRoom = currentRoom,
            IsGameCompleted = session?.Status == GameStatus.Completed
        };

        return Ok(result);
    }

    /// <summary>
    /// Termine une session de jeu
    /// </summary>
    /// <param name="sessionId">ID de la session</param>
    /// <param name="reason">Raison de l'arrêt (abandon, mort, etc.)</param>
    [HttpPost("session/{sessionId}/end")]
    public async Task<ActionResult<GameSession>> EndSession(int sessionId, [FromQuery] string reason = "Abandon")
    {
        try
        {
            var status = reason.ToLower() == "death" ? GameStatus.Failed : GameStatus.Failed;
            var session = await _gameSessionService.EndSessionAsync(sessionId, status);
            
            _logger.LogInformation("Session {SessionId} terminée : {Reason}", sessionId, reason);
            return Ok(session);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Récupère le classement des joueurs
    /// </summary>
    /// <returns>Classement des meilleurs scores</returns>
    [HttpGet("leaderboard")]
    public ActionResult<object> GetLeaderboard()
    {
        // Cette méthode sera implémentée plus tard pour la version admin
        // Pour l'instant, on retourne un placeholder
        var leaderboard = new
        {
            Message = "Classement à venir dans la version admin",
            TopPlayers = new object[] { }
        };

        return Ok(leaderboard);
    }

    /// <summary>
    /// Détermine les actions disponibles selon le type de salle
    /// </summary>
    private static string[] GetAvailableActions(Room? room)
    {
        if (room == null) return Array.Empty<string>();

        return room.Type switch
        {
            RoomType.Monster => new[] { "Combat", "Flee" },
            RoomType.Treasure => new[] { "OpenChest", "Search", "Ignore" },
            RoomType.Trap => new[] { "Search", "Bypass", "Ignore" },
            RoomType.Empty => new[] { "Search", "Ignore" },
            RoomType.Rest => new[] { "Rest", "Ignore" },
            RoomType.Mystery => new[] { "Investigate", "Flee" },
            _ => new[] { "Combat", "Flee", "Search", "Ignore" } // Fallback pour compatibilité
        };
    }
}