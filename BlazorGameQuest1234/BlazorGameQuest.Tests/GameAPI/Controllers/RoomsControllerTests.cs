using GameAPI.Controllers;
using DataAccess.Data;
using SharedModels.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace BlazorGameQuest.Tests.GameAPI.Controllers;

/// <summary>
/// Tests unitaires pour le RoomsController
/// Vérifie les opérations CRUD sur les salles
/// </summary>
public class RoomsControllerTests : IDisposable
{
    private readonly GameDbContext _context;
    private readonly RoomsController _controller;

    public RoomsControllerTests()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new GameDbContext(options);
        _controller = new RoomsController(_context);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var dungeon = new Dungeon
        {
            Id = 1,
            Name = "Test Dungeon",
            Rooms = new List<Room>()
        };

        var rooms = new List<Room>
        {
            new Room { Id = 1, DungeonId = 1, RoomNumber = 1, Description = "Room 1" },
            new Room { Id = 2, DungeonId = 1, RoomNumber = 2, Description = "Room 2" },
            new Room { Id = 3, DungeonId = 1, RoomNumber = 3, Description = "Room 3" }
        };

        _context.Dungeons.Add(dungeon);
        _context.Rooms.AddRange(rooms);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetRooms_ReturnsPaginatedRooms()
    {
        // Act
        var result = await _controller.GetRooms(1, 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var rooms = Assert.IsAssignableFrom<IEnumerable<Room>>(okResult.Value);
        Assert.Equal(3, rooms.Count());
    }

    [Fact]
    public async Task GetRoom_WithValidId_ReturnsRoom()
    {
        // Act
        var result = await _controller.GetRoom(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var room = Assert.IsType<Room>(okResult.Value);
        Assert.Equal(1, room.Id);
    }

    [Fact]
    public async Task GetRoom_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.GetRoom(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetRoomsByDungeon_ReturnsRoomsForDungeon()
    {
        // Act
        var result = await _controller.GetRoomsByDungeon(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var rooms = Assert.IsAssignableFrom<IEnumerable<Room>>(okResult.Value);
        Assert.Equal(3, rooms.Count());
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
