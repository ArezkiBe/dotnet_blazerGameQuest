using GameAPI.Controllers;
using DataAccess.Data;
using SharedModels.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace BlazorGameQuest.Tests.GameAPI.Controllers;

/// <summary>
/// Tests unitaires pour le DungeonsController
/// Vérifie les opérations CRUD sur les donjons
/// </summary>
public class DungeonsControllerTests : IDisposable
{
    private readonly GameDbContext _context;
    private readonly DungeonsController _controller;

    public DungeonsControllerTests()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new GameDbContext(options);
        _controller = new DungeonsController(_context);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var dungeons = new List<Dungeon>
        {
            new Dungeon { Id = 1, Name = "Donjon 1" },
            new Dungeon { Id = 2, Name = "Donjon 2" },
            new Dungeon { Id = 3, Name = "Donjon 3" }
        };

        _context.Dungeons.AddRange(dungeons);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetDungeons_ReturnsPaginatedDungeons()
    {
        // Act
        var result = await _controller.GetDungeons(1, 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dungeons = Assert.IsAssignableFrom<IEnumerable<Dungeon>>(okResult.Value);
        Assert.Equal(3, dungeons.Count());
    }

    [Fact]
    public async Task GetDungeon_WithValidId_ReturnsDungeon()
    {
        // Act
        var result = await _controller.GetDungeon(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dungeon = Assert.IsType<Dungeon>(okResult.Value);
        Assert.Equal(1, dungeon.Id);
    }

    [Fact]
    public async Task GetDungeon_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.GetDungeon(999);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
