using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataAccess.Data;
using SharedModels.Models;

namespace AuthenticationServices.Controllers;

/// <summary>
/// API Controller pour la gestion des utilisateurs
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Users")]
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
    /// Récupère tous les utilisateurs actifs
    /// </summary>
    /// <returns>Liste des utilisateurs actifs</returns>
    /// <response code="200">Retourne la liste des utilisateurs</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        var users = await _context.Users
            .Where(u => u.IsActive)
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
    /// Récupère un utilisateur par son nom d'utilisateur
    /// </summary>
    [HttpGet("by-username/{username}")]
    public async Task<ActionResult<User>> GetUserByUsername(string username)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
        
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
}