using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataAccess.Data;
using SharedModels.Models;

namespace AuthenticationServices.Controllers;

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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Dungeon>>> GetDungeons()
    {
        var dungeons = await _context.Dungeons.ToListAsync();
        return Ok(dungeons);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Dungeon>> GetDungeon(int id)
    {
        var dungeon = await _context.Dungeons.FindAsync(id);
        return dungeon == null ? NotFound() : Ok(dungeon);
    }

    [HttpPost]
    public async Task<ActionResult<Dungeon>> CreateDungeon(Dungeon dungeon)
    {
        _context.Dungeons.Add(dungeon);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDungeon), new { id = dungeon.Id }, dungeon);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDungeon(int id, Dungeon dungeon)
    {
        if (id != dungeon.Id)
            return BadRequest();

        _context.Entry(dungeon).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

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