using Xunit;
using Microsoft.EntityFrameworkCore;
using GameAPI.Services;
using DataAccess.Data;
using SharedModels.Models;

namespace BlazorGameQuest.Tests.Services;

/// <summary>
/// Tests unitaires pour les services essentiels
/// </summary>
public class ServicesTests : IDisposable
{
    private readonly GameDbContext _context;

    public ServicesTests()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new GameDbContext(options);
    }

    [Fact]
    public async Task DungeonGeneratorService_GenereDonjonCorrectement()
    {
        // Arrange
        var service = new DungeonGeneratorService(_context);

        // Act
        var dungeon = await service.GenerateDungeonAsync(difficultyLevel: 2, roomCount: 5);

        // Assert
        Assert.NotNull(dungeon);
        Assert.Equal(2, dungeon.DifficultyLevel);
        Assert.Equal(5, dungeon.TotalRooms);
        Assert.NotEmpty(dungeon.Name);
        Assert.NotEmpty(dungeon.Description);
        
        // Vérifier que les salles sont créées
        var rooms = await _context.Rooms.Where(r => r.DungeonId == dungeon.Id).ToListAsync();
        Assert.Equal(5, rooms.Count);
    }

    [Fact]
    public void DungeonGeneratorService_GenereSalleAleatoire()
    {
        // Arrange
        var service = new DungeonGeneratorService(_context);

        // Act
        var room = service.GenerateRandomRoom(dungeonId: 1, roomNumber: 1, baseDifficulty: 3);

        // Assert
        Assert.Equal(1, room.DungeonId);
        Assert.Equal(1, room.RoomNumber);
        Assert.NotEmpty(room.Title);
        Assert.NotEmpty(room.Description);
        Assert.InRange(room.Difficulty, 1, 5);
        Assert.InRange(room.CombatSuccessRate, 0, 100);
        Assert.True(room.CombatReward > 0);
    }

    [Fact]
    public async Task GameSessionService_CreNouvelleSession()
    {
        // Arrange
        var dungeonService = new DungeonGeneratorService(_context);
        var gameSessionService = new GameSessionService(_context, dungeonService);
        
        var player = new Player { Username = "TestPlayer", Score = 0 };
        _context.Players.Add(player);
        await _context.SaveChangesAsync();

        // Act
        var session = await gameSessionService.StartNewAdventureAsync(player.Id, difficultyLevel: 2);

        // Assert
        Assert.NotNull(session);
        Assert.Equal(player.Id, session.PlayerId);
        Assert.Equal(GameStatus.Active, session.Status);
        Assert.Equal(100, session.CurrentHP);
        Assert.Equal(1, session.Level);
    }

    [Theory]
    [InlineData(1, 3)]
    [InlineData(2, 4)]
    [InlineData(3, 5)]
    public async Task DungeonGeneratorService_AvecDifferentsParametres_CreeBonsDonjons(int difficulty, int roomCount)
    {
        // Arrange
        var service = new DungeonGeneratorService(_context);

        // Act
        var dungeon = await service.GenerateDungeonAsync(difficulty, roomCount);

        // Assert
        Assert.Equal(difficulty, dungeon.DifficultyLevel);
        Assert.Equal(roomCount, dungeon.TotalRooms);
        
        var rooms = await _context.Rooms.Where(r => r.DungeonId == dungeon.Id).ToListAsync();
        Assert.Equal(roomCount, rooms.Count);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}