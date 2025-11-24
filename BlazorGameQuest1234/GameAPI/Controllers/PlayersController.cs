using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataAccess.Data;
using SharedModels.Models;

namespace GameAPI.Controllers;

/// <summary>
/// API Controller pour la gestion des joueurs
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Players")]
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
}