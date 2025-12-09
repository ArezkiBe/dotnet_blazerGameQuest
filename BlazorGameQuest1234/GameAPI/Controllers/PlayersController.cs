using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataAccess.Data;
using SharedModels.Models;
using Microsoft.AspNetCore.Authorization;

namespace GameAPI.Controllers;

/// <summary>
/// API Controller pour la gestion des joueurs
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Players")]
[Authorize] // Utilisateurs authentifiés
public class PlayersController : ControllerBase
{
    private readonly GameDbContext _context;
    private readonly ILogger<PlayersController> _logger;

    public PlayersController(GameDbContext context, ILogger<PlayersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Récupère tous les joueurs triés par score
    /// </summary>
    /// <returns>Liste des joueurs triés par score décroissant</returns>
    /// <response code="200">Retourne la liste des joueurs</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Player>>> GetPlayers()
    {
        var players = await _context.Players
            .OrderByDescending(p => p.Score)
            .ToListAsync();

        return Ok(players);
    }

    /// <summary>
    /// Récupère un joueur par son ID
    /// </summary>
    /// <param name="id">Identifiant du joueur</param>
    /// <returns>Le joueur correspondant</returns>
    /// <response code="200">Joueur trouvé</response>
    /// <response code="404">Joueur non trouvé</response>
    [HttpGet("{id}")]
    public async Task<ActionResult<Player>> GetPlayer(int id)
    {
        var player = await _context.Players.FindAsync(id);
        return player == null ? NotFound() : Ok(player);
    }

    /// <summary>
    /// Récupère un joueur par l'ID de son utilisateur
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur</param>
    /// <returns>Le profil joueur lié à cet utilisateur</returns>
    /// <response code="200">Profil joueur trouvé</response>
    /// <response code="404">Aucun profil joueur pour cet utilisateur</response>
    [HttpGet("by-user/{userId}")]
    public async Task<ActionResult<Player>> GetPlayerByUserId(int userId)
    {
        var player = await _context.Players
            .FirstOrDefaultAsync(p => p.UserId == userId);

        return player == null ? NotFound() : Ok(player);
    }

    /// <summary>
    /// Crée un nouveau joueur
    /// </summary>
    /// <param name="player">Données du joueur à créer</param>
    /// <returns>Le joueur créé</returns>
    /// <response code="201">Joueur créé avec succès</response>
    /// <response code="400">Utilisateur invalide</response>
    /// <response code="409">Profil joueur déjà existant pour cet utilisateur</response>
    [HttpPost]
    public async Task<ActionResult<Player>> CreatePlayer(Player player)
    {
        // Vérifier que l'utilisateur existe
        var user = await _context.Users.FindAsync(player.UserId);
        if (user == null || user.Role != UserRole.Player)
            return BadRequest("Utilisateur invalide");

        // Vérifier l'unicité
        var exists = await _context.Players.AnyAsync(p => p.UserId == player.UserId);
        if (exists)
            return Conflict("Profil joueur déjà existant");

        player.CreatedAt = DateTime.UtcNow;
        _context.Players.Add(player);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPlayer), new { id = player.Id }, player);
    }

    /// <summary>
    /// Met à jour un joueur existant
    /// </summary>
    /// <param name="id">Identifiant du joueur à modifier</param>
    /// <param name="player">Nouvelles données du joueur</param>
    /// <returns>Aucun contenu en cas de succès</returns>
    /// <response code="204">Joueur mis à jour avec succès</response>
    /// <response code="400">Données invalides ou ID incohérent</response>
    /// <response code="404">Joueur non trouvé</response>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePlayer(int id, Player player)
    {
        if (id != player.Id)
            return BadRequest();

        _context.Entry(player).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Supprime un joueur
    /// </summary>
    /// <param name="id">Identifiant du joueur à supprimer</param>
    /// <returns>Aucun contenu en cas de succès</returns>
    /// <response code="204">Joueur supprimé avec succès</response>
    /// <response code="404">Joueur non trouvé</response>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlayer(int id)
    {
        var player = await _context.Players.FindAsync(id);
        if (player == null)
            return NotFound();

        _context.Players.Remove(player);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Récupère le classement des 10 meilleurs joueurs
    /// </summary>
    /// <returns>Top 10 des joueurs par score</returns>
    /// <response code="200">Retourne le classement des joueurs</response>
    [HttpGet("leaderboard")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<object>>> GetLeaderboard()
    {
        var leaderboard = await _context.Players
            .OrderByDescending(p => p.Score)
            .Take(10)
            .Select(p => new
            {
                p.Id,
                p.UserId,
                p.Username,
                p.Score
            })
            .ToListAsync();

        return Ok(leaderboard);
    }

    /// <summary>
    /// Récupère tous les joueurs avec leurs informations utilisateur (Admin uniquement)
    /// </summary>
    /// <returns>Liste détaillée des joueurs avec informations utilisateur</returns>
    [HttpGet("admin/detailed")]
    public async Task<ActionResult<IEnumerable<object>>> GetPlayersDetailed()
    {
        var players = await _context.Players
            .Include(p => p.User)
            .Include(p => p.GameSessions)
            .OrderByDescending(p => p.Score)
            .Select(p => new
            {
                Player = p,
                User = new
                {
                    p.User.Id,
                    p.User.Username,
                    p.User.Email,
                    p.User.IsActive,
                    p.User.Role,
                    p.User.CreatedAt
                },
                SessionsCount = p.GameSessions.Count,
                LastSession = p.GameSessions
                    .OrderByDescending(gs => gs.StartedAt)
                    .Select(gs => gs.StartedAt)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(players);
    }

    /// <summary>
    /// Récupère les statistiques détaillées des joueurs pour l'admin
    /// </summary>
    [HttpGet("admin/statistics")]
    public async Task<ActionResult<object>> GetPlayerStatistics()
    {
        var totalPlayers = await _context.Players.CountAsync();
        var activePlayers = await _context.Players
            .Include(p => p.User)
            .CountAsync(p => p.User.IsActive);
        
        var topScore = await _context.Players.MaxAsync(p => (int?)p.Score) ?? 0;
        var averageScore = await _context.Players.AverageAsync(p => (double?)p.Score) ?? 0;
        var totalScore = await _context.Players.SumAsync(p => p.Score);
        
        var totalSessions = await _context.GameSessions.CountAsync();
        var averageSessionsPerPlayer = totalPlayers > 0 ? (double)totalSessions / totalPlayers : 0;

        var topPlayers = await _context.Players
            .OrderByDescending(p => p.Score)
            .Take(10)
            .Select(p => new
            {
                p.Id,
                p.Username,
                p.Score
            })
            .ToListAsync();

        var stats = new
        {
            TotalPlayers = totalPlayers,
            ActivePlayers = activePlayers,
            InactivePlayers = totalPlayers - activePlayers,
            TopScore = topScore,
            AverageScore = (int)averageScore,
            TotalScore = totalScore,
            TotalSessions = totalSessions,
            AverageSessionsPerPlayer = Math.Round(averageSessionsPerPlayer, 1),
            TopPlayers = topPlayers
        };

        return Ok(stats);
    }

    /// <summary>
    /// Exporte les données des joueurs au format JSON (Admin uniquement)
    /// </summary>
    [HttpGet("admin/export")]
    public async Task<ActionResult<object>> ExportPlayersData([FromQuery] bool includeInactive = false)
    {
        var query = _context.Players
            .Include(p => p.User)
            .Include(p => p.GameSessions)
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(p => p.User.IsActive);
        }

        var players = await query
            .OrderByDescending(p => p.Score)
            .Select(p => new
            {
                PlayerId = p.Id,
                Username = p.Username,
                Email = p.User.Email,
                Score = p.Score,
                CurrentRoom = p.CurrentRoom,
                IsActive = p.User.IsActive,
                CreatedAt = p.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                LastLogin = p.User.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                TotalSessions = p.GameSessions.Count,
                TotalPlaytime = p.GameSessions
                    .Where(gs => gs.CompletedAt.HasValue)
                    .Sum(gs => (gs.CompletedAt!.Value - gs.StartedAt).TotalMinutes)
            })
            .ToListAsync();

        var exportData = new
        {
            ExportDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
            TotalPlayers = players.Count,
            IncludesInactive = includeInactive,
            Players = players
        };

        return Ok(exportData);
    }

    /// <summary>
    /// Récupère l'historique des sessions de jeu d'un joueur
    /// </summary>
    /// <param name="playerId">Identifiant du joueur</param>
    /// <returns>Liste des sessions de jeu du joueur</returns>
    /// <response code="200">Sessions trouvées</response>
    /// <response code="404">Joueur non trouvé</response>
    [HttpGet("{playerId}/sessions")]
    public async Task<ActionResult<IEnumerable<GameSession>>> GetPlayerSessions(int playerId)
    {
        var player = await _context.Players.FindAsync(playerId);
        if (player == null)
            return NotFound();

        var sessions = await _context.GameSessions
            .Where(gs => gs.PlayerId == playerId)
            .OrderByDescending(gs => gs.StartedAt)
            .ToListAsync();

        return Ok(sessions);
    }

    /// <summary>
    /// Récupère l'historique des sessions par ID utilisateur
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur</param>
    /// <returns>Liste des sessions de jeu de l'utilisateur</returns>
    /// <response code="200">Sessions trouvées</response>
    /// <response code="404">Utilisateur/joueur non trouvé</response>
    [HttpGet("user/{userId}/sessions")]
    public async Task<ActionResult<IEnumerable<GameSession>>> GetPlayerSessionsByUserId(int userId)
    {
        var player = await _context.Players
            .FirstOrDefaultAsync(p => p.UserId == userId);
        
        if (player == null)
            return NotFound();

        var sessions = await _context.GameSessions
            .Where(gs => gs.PlayerId == player.Id)
            .OrderByDescending(gs => gs.StartedAt)
            .ToListAsync();

        return Ok(sessions);
    }

    /// <summary>
    /// Récupère les données du joueur par nom d'utilisateur
    /// </summary>
    /// <param name="username">Nom d'utilisateur</param>
    /// <returns>Données du joueur avec ses sessions</returns>
    [HttpGet("by-username/{username}")]
    public async Task<ActionResult<object>> GetPlayerByUsername(string username)
    {
        // Étape 1: Vérifier si l'utilisateur existe dans la base locale
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);
            
        // Étape 2: Si l'utilisateur n'existe pas localement, le créer automatiquement
        if (user == null)
        {
            // Créer un nouvel utilisateur joueur par défaut
            user = new User
            {
                Username = username,
                Email = $"{username}@keycloak.local", // Email par défaut
                Role = UserRole.Player,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        // Étape 3: Vérifier si le profil joueur existe
        var player = await _context.Players
            .FirstOrDefaultAsync(p => p.UserId == user.Id);
            
        // Étape 4: Si le profil joueur n'existe pas, le créer automatiquement
        if (player == null)
        {
            player = new Player
            {
                UserId = user.Id,
                Username = username,
                Score = 0,
                CurrentRoom = 1,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Players.Add(player);
            await _context.SaveChangesAsync();
        }

        // Étape 5: Récupérer les sessions de jeu
        var sessions = await _context.GameSessions
            .Where(gs => gs.PlayerId == player.Id)
            .OrderByDescending(gs => gs.StartedAt)
            .ToListAsync();

        return Ok(new { Player = player, Sessions = sessions });
    }
}