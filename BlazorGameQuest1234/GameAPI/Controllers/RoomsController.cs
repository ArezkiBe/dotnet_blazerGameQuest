using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataAccess.Data;
using SharedModels.Models;
using Microsoft.AspNetCore.Authorization;

namespace GameAPI.Controllers;

/// <summary>
/// Contrôleur API pour la gestion des salles de donjon
/// Fournit les opérations CRUD pour les salles individuelles
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Rooms")]
[Authorize] // Utilisateurs authentifiés
public class RoomsController : ControllerBase
{
    private readonly GameDbContext _context;

    public RoomsController(GameDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Récupère une liste paginée des salles
    /// </summary>
    /// <param name="page">Numéro de page (défaut: 1)</param>
    /// <param name="pageSize">Nombre d'éléments par page (défaut: 50)</param>
    /// <returns>Liste des salles pour la page demandée</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Room>>> GetRooms([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var rooms = await _context.Rooms
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return Ok(rooms);
    }

    /// <summary>
    /// Récupère une salle par son ID
    /// </summary>
    /// <param name="id">Identifiant de la salle</param>
    /// <returns>La salle correspondante</returns>
    /// <response code="200">Salle trouvée</response>
    /// <response code="400">ID invalide</response>
    /// <response code="404">Salle non trouvée</response>
    [HttpGet("{id}")]
    public async Task<ActionResult<Room>> GetRoom(int id)
    {
        if (id <= 0)
        {
            return BadRequest("Invalid room ID");
        }

        var room = await _context.Rooms.FindAsync(id);
        return room == null ? NotFound($"Room with ID {id} not found") : Ok(room);
    }

    /// <summary>
    /// Récupère toutes les salles d'un donjon spécifique
    /// </summary>
    /// <param name="dungeonId">ID du donjon</param>
    /// <returns>Liste des salles du donjon</returns>
    [HttpGet("by-dungeon/{dungeonId}")]
    public async Task<ActionResult<IEnumerable<Room>>> GetRoomsByDungeon(int dungeonId)
    {
        var rooms = await _context.Rooms
            .Where(r => r.DungeonId == dungeonId)
            .ToListAsync();
        return Ok(rooms);
    }

    /// <summary>
    /// Crée une nouvelle salle (admin seulement)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "administrateur")]
    public async Task<ActionResult<Room>> CreateRoom(Room room)
    {
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRoom), new { id = room.Id }, room);
    }

    /// <summary>
    /// Met à jour une salle existante (admin seulement)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "administrateur")]
    public async Task<IActionResult> UpdateRoom(int id, Room room)
    {
        if (id != room.Id)
            return BadRequest();

        _context.Entry(room).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Supprime une salle (admin seulement)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "administrateur")]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room == null)
            return NotFound();

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}