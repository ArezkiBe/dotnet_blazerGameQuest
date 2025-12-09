using GameAPI.Services;
using SharedModels.Models;
using DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace BlazorGameQuest.Tests.GameAPI.Services;

/// <summary>
/// Tests unitaires pour le DungeonGeneratorService
/// Vérifie la génération procédurale des donjons
/// </summary>
public class DungeonGeneratorServiceTests : IDisposable
{
    private readonly GameDbContext _context;
    private readonly IDungeonGeneratorService _service;

    public DungeonGeneratorServiceTests()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new GameDbContext(options);
        _service = new DungeonGeneratorService(_context);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task GenerateDungeonAsync_CreatesCorrectNumberOfRooms(int difficulty)
    {
        // Act
        var dungeon = await _service.GenerateDungeonAsync(difficulty);

        // Assert
        Assert.NotNull(dungeon);
        Assert.Equal(5, dungeon.Rooms.Count);
    }

    [Fact]
    public async Task GenerateDungeonAsync_RoomsHaveCorrectSequence()
    {
        // Act
        var dungeon = await _service.GenerateDungeonAsync(2);

        // Assert
        var rooms = dungeon.Rooms.OrderBy(r => r.RoomNumber).ToList();
        
        for (int i = 0; i < rooms.Count; i++)
        {
            Assert.Equal(i + 1, rooms[i].RoomNumber);
        }
    }

    [Fact]
    public async Task GenerateDungeonAsync_AllRoomsHaveDescriptions()
    {
        // Act
        var dungeon = await _service.GenerateDungeonAsync(2);

        // Assert
        foreach (var room in dungeon.Rooms)
        {
            Assert.False(string.IsNullOrWhiteSpace(room.Description));
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task GenerateDungeonAsync_CreatesValidName(int difficulty)
    {
        // Act
        var dungeon = await _service.GenerateDungeonAsync(difficulty);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(dungeon.Name));
    }

    [Fact]
    public async Task GenerateDungeonAsync_RoomsHaveDescriptions()
    {
        // Act
        var dungeon = await _service.GenerateDungeonAsync(2);

        // Assert
        foreach (var room in dungeon.Rooms)
        {
            Assert.False(string.IsNullOrWhiteSpace(room.Description));
        }
    }

    [Fact]
    public void GenerateRandomRoom_CreatesValidRoom()
    {
        // Act
        var room = _service.GenerateRandomRoom(1, 1, 2);

        // Assert
        Assert.NotNull(room);
        Assert.Equal(1, room.DungeonId);
        Assert.Equal(1, room.RoomNumber);
        Assert.False(string.IsNullOrWhiteSpace(room.Description));
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
