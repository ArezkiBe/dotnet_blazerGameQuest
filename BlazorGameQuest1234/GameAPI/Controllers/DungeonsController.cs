using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataAccess.Data;
using SharedModels.Models;

namespace GameAPI.Controllers;

/// <summary>
/// API Controller pour la gestion des donjons
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Dungeons")]
public class DungeonsController : ControllerBase
{
    private readonly GameDbContext _context;

    public DungeonsController(GameDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Récupère une liste paginée de donjons
    /// </summary>
    /// <param name="page">Numéro de page (défaut: 1)</param>
    /// <param name="pageSize">Nombre d'éléments par page (défaut: 50, max: 100)</param>
    /// <returns>Liste paginée des donjons</returns>
    /// <response code="200">Donjons récupérés avec succès</response>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Dungeon>>> GetDungeons([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var dungeons = await _context.Dungeons
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return Ok(dungeons);
    }

    /// <summary>
    /// Récupère un donjon par son ID
    /// </summary>
    /// <param name="id">Identifiant du donjon</param>
    /// <returns>Le donjon correspondant</returns>
    /// <response code="200">Donjon trouvé</response>
    /// <response code="404">Donjon non trouvé</response>
    [HttpGet("{id}")]
    public async Task<ActionResult<Dungeon>> GetDungeon(int id)
    {
        var dungeon = await _context.Dungeons.FindAsync(id);
        return dungeon == null ? NotFound() : Ok(dungeon);
    }

    /// <summary>
    /// Crée un nouveau donjon
    /// </summary>
    /// <param name="dungeon">Données du donjon à créer</param>
    /// <returns>Le donjon créé</returns>
    /// <response code="201">Donjon créé avec succès</response>
    /// <response code="400">Données invalides</response>
    [HttpPost]
    public async Task<ActionResult<Dungeon>> CreateDungeon(Dungeon dungeon)
    {
        _context.Dungeons.Add(dungeon);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDungeon), new { id = dungeon.Id }, dungeon);
    }

    /// <summary>
    /// Met à jour un donjon existant
    /// </summary>
    /// <param name="id">Identifiant du donjon à modifier</param>
    /// <param name="dungeon">Nouvelles données du donjon</param>
    /// <returns>Aucun contenu en cas de succès</returns>
    /// <response code="204">Donjon mis à jour avec succès</response>
    /// <response code="400">Données invalides ou ID incohérent</response>
    /// <response code="404">Donjon non trouvé</response>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDungeon(int id, Dungeon dungeon)
    {
        if (id != dungeon.Id)
            return BadRequest();

        _context.Entry(dungeon).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Supprime un donjon
    /// </summary>
    /// <param name="id">Identifiant du donjon à supprimer</param>
    /// <returns>Aucun contenu en cas de succès</returns>
    /// <response code="204">Donjon supprimé avec succès</response>
    /// <response code="404">Donjon non trouvé</response>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDungeon(int id)
    {
        var dungeon = await _context.Dungeons.FindAsync(id);
        if (dungeon == null)
            return NotFound();

        _context.Dungeons.Remove(dungeon);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}