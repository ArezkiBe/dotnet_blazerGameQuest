using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataAccess.Data;
using SharedModels.Models;
using Microsoft.AspNetCore.Authorization;

namespace GameAPI.Controllers;

/// <summary>
/// API Controller pour la gestion des utilisateurs
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Users")]
[Authorize] // Utilisateurs authentifiés, vérification admin dans les méthodes
public class UsersController : ControllerBase
{
    private readonly GameDbContext _context;
    private readonly ILogger<UsersController> _logger;

    public UsersController(GameDbContext context, ILogger<UsersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Vérifie si l'utilisateur actuel est admin
    /// </summary>
    private bool IsCurrentUserAdmin()
    {
        var username = User.FindFirst("preferred_username")?.Value ?? "";
        return username.ToLower() == "admin" || User.IsInRole("administrateur");
    }

    /// <summary>
    /// Récupère tous les utilisateurs (actifs et inactifs)
    /// </summary>
    /// <returns>Liste de tous les utilisateurs</returns>
    /// <response code="200">Retourne la liste des utilisateurs</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        // Vérification admin manuelle car nécessaire pour PlayerManagement
        if (!IsCurrentUserAdmin())
        {
            return Forbid();
        }

        // Retourner TOUS les utilisateurs (actifs et inactifs) pour la gestion
        var users = await _context.Users
            .OrderBy(u => u.Username)
            .ToListAsync();
        
        return Ok(users);
    }

    /// <summary>
    /// Récupère un utilisateur par son ID
    /// </summary>
    /// <param name="id">ID de l'utilisateur</param>
    /// <returns>L'utilisateur correspondant</returns>
    /// <response code="200">Utilisateur trouvé</response>
    /// <response code="404">Utilisateur non trouvé</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        return user == null ? NotFound() : Ok(user);
    }

    /// <summary>
    /// Récupère un utilisateur par son nom d'utilisateur (actif ou inactif)
    /// </summary>
    [HttpGet("by-username/{username}")]
    public async Task<ActionResult<User>> GetUserByUsername(string username)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);
        
        return user == null ? NotFound() : Ok(user);
    }

    /// <summary>
    /// Crée un nouvel utilisateur
    /// </summary>
    /// <param name="user">Données de l'utilisateur à créer</param>
    /// <returns>L'utilisateur créé</returns>
    /// <response code="201">Utilisateur créé avec succès</response>
    /// <response code="409">Nom d'utilisateur ou email déjà utilisé</response>
    [HttpPost]
    [Authorize(Roles = "administrateur")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<User>> CreateUser(User user)
    {
        // Vérifier l'unicité
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == user.Username || u.Email == user.Email);

        if (existingUser != null)
            return Conflict("Nom d'utilisateur ou email déjà utilisé");

        user.CreatedAt = DateTime.UtcNow;
        user.IsActive = true;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    /// <summary>
    /// Met à jour un utilisateur existant
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "administrateur")]
    public async Task<IActionResult> UpdateUser(int id, User user)
    {
        if (id != user.Id)
            return BadRequest();

        var existingUser = await _context.Users.FindAsync(id);
        if (existingUser == null)
            return NotFound();

        // Vérifier l'unicité
        var duplicate = await _context.Users
            .AnyAsync(u => u.Id != id && (u.Username == user.Username || u.Email == user.Email));

        if (duplicate)
            return Conflict();

        existingUser.Username = user.Username;
        existingUser.Email = user.Email;
        existingUser.Role = user.Role;
        existingUser.IsActive = user.IsActive;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Supprime un utilisateur (suppression logique)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "administrateur")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        user.IsActive = false;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Récupère les statistiques des utilisateurs
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<object>> GetUserStatistics()
    {
        var stats = new
        {
            TotalUsers = await _context.Users.CountAsync(),
            ActiveUsers = await _context.Users.CountAsync(u => u.IsActive),
            Administrators = await _context.Users.CountAsync(u => u.Role == UserRole.Administrator),
            Players = await _context.Users.CountAsync(u => u.Role == UserRole.Player)
        };

        return Ok(stats);
    }

    /// <summary>
    /// Active ou désactive un utilisateur (Admin uniquement)
    /// </summary>
    /// <param name="id">ID de l'utilisateur</param>
    /// <param name="isActive">Nouveau statut d'activation</param>
    /// <returns>Utilisateur mis à jour</returns>
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] bool isActive)
    {
        // Vérification admin manuelle
        if (!IsCurrentUserAdmin())
        {
            return Forbid();
        }

        var user = await _context.Users.FindAsync(id);
        
        if (user == null)
        {
            return NotFound(new { Message = "Utilisateur non trouvé" });
        }

        // Empêcher la désactivation du dernier administrateur
        if (!isActive && user.Role == UserRole.Administrator)
        {
            var adminCount = await _context.Users.CountAsync(u => u.Role == UserRole.Administrator && u.IsActive);
            if (adminCount <= 1)
            {
                return BadRequest(new { Message = "Impossible de désactiver le dernier administrateur" });
            }
        }

        user.IsActive = isActive;
        await _context.SaveChangesAsync();

        return Ok(new { Message = $"Utilisateur {(isActive ? "activé" : "désactivé")} avec succès", User = user });
    }

    /// <summary>
    /// Récupère tous les utilisateurs incluant les inactifs (Admin uniquement)
    /// </summary>
    /// <returns>Liste de tous les utilisateurs</returns>
    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
    {
        var users = await _context.Users
            .OrderBy(u => u.Username)
            .ToListAsync();
        
        return Ok(users);
    }

    /// <summary>
    /// Récupère les statistiques détaillées pour le dashboard admin
    /// </summary>
    [HttpGet("dashboard-stats")]
    [Authorize(Roles = "administrateur")]
    public async Task<ActionResult<object>> GetDashboardStatistics()
    {
        var totalUsers = await _context.Users.CountAsync();
        var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
        var totalPlayers = await _context.Players.CountAsync();
        var totalSessions = await _context.GameSessions.CountAsync();
        
        var topPlayer = await _context.Players
            .OrderByDescending(p => p.Score)
            .FirstOrDefaultAsync();
            
        var averageScore = await _context.Players.AverageAsync(p => (double?)p.Score) ?? 0;
        
        var recentActivity = await _context.GameSessions
            .OrderByDescending(gs => gs.StartedAt)
            .Take(5)
            .Include(gs => gs.Player)
            .Select(gs => new {
                PlayerName = gs.Player!.Username,
                Score = gs.TotalScore,
                Date = gs.StartedAt,
                Duration = gs.CompletedAt.HasValue ? 
                    (gs.CompletedAt.Value - gs.StartedAt).TotalMinutes : 0
            })
            .ToListAsync();

        var stats = new
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            TotalPlayers = totalPlayers,
            TotalSessions = totalSessions,
            TopPlayer = topPlayer?.Username ?? "Aucun",
            TopScore = topPlayer?.Score ?? 0,
            AverageScore = (int)averageScore,
            RecentActivity = recentActivity
        };

        return Ok(stats);
    }
}