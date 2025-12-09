using GameAPI.Services;
using DataAccess.Data;
using SharedModels.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorGameQuest.Tests.GameAPI.Services;

/// <summary>
/// Tests unitaires pour le GameSessionService
/// Vérifie la logique métier des sessions de jeu
/// </summary>
public class GameSessionServiceTests : IDisposable
{
    private readonly GameDbContext _context;
    private readonly IGameSessionService _service;
    private readonly IDungeonGeneratorService _dungeonGenerator;

    public GameSessionServiceTests()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new GameDbContext(options);
        _dungeonGenerator = new DungeonGeneratorService(_context);
        _service = new GameSessionService(_context, _dungeonGenerator);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var user = new User
        {
            Id = 1,
            Username = "testplayer",
            Email = "test@test.com",
            Role = UserRole.Player,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var player = new Player
        {
            Id = 1,
            UserId = 1,
            Username = "testplayer",
            Score = 0,
            CurrentRoom = 1,
            KeycloakUserId = "kc1",
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        _context.Players.Add(player);
        _context.SaveChanges();
    }

    [Fact]
    public async Task StartNewAdventureAsync_CreatesSessionWithDungeon()
    {
        // Act
        var session = await _service.StartNewAdventureAsync(1, 2);

        // Assert
        Assert.NotNull(session);
        Assert.Equal(1, session.PlayerId);
        Assert.Equal(GameStatus.Active, session.Status);
        Assert.Equal(1, session.CurrentRoomNumber);
        Assert.Equal(100, session.CurrentHP);
        Assert.NotEqual(0, session.DungeonId);

        // Vérifier que le donjon et les salles ont été créés
        var dungeon = await _context.Dungeons
            .Include(d => d.Rooms)
            .FirstOrDefaultAsync(d => d.Id == session.DungeonId);

        Assert.NotNull(dungeon);
        Assert.Equal(5, dungeon.Rooms.Count); // 5 salles par donjon
    }

    [Fact]
    public async Task StartNewAdventureAsync_WithInvalidPlayer_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.StartNewAdventureAsync(999, 2)
        );
    }

    [Fact]
    public async Task GetActiveSessionAsync_WithValidId_ReturnsSession()
    {
        // Arrange
        var session = await _service.StartNewAdventureAsync(1, 2);

        // Act
        var retrievedSession = await _service.GetActiveSessionAsync(session.Id);

        // Assert
        Assert.NotNull(retrievedSession);
        Assert.Equal(session.Id, retrievedSession.Id);
        Assert.Equal(GameStatus.Active, retrievedSession.Status);
    }

    [Fact]
    public async Task GetActiveSessionAsync_WithCompletedSession_ReturnsNull()
    {
        // Arrange
        var session = await _service.StartNewAdventureAsync(1, 2);
        await _service.EndSessionAsync(session.Id, GameStatus.Completed);

        // Act
        var retrievedSession = await _service.GetActiveSessionAsync(session.Id);

        // Assert
        Assert.Null(retrievedSession);
    }

    [Fact]
    public async Task PerformActionAsync_CombatAction_UpdatesSession()
    {
        // Arrange
        var session = await _service.StartNewAdventureAsync(1, 2);

        // Act
        var action = await _service.PerformActionAsync(session.Id, ActionType.Combat);

        // Assert
        Assert.NotNull(action);
        Assert.Equal(ActionType.Combat, action.ActionType);
        Assert.Equal(session.Id, action.GameSessionId);
    }

    [Fact]
    public async Task MoveToNextRoomAsync_IncreasesRoomNumber()
    {
        // Arrange
        var session = await _service.StartNewAdventureAsync(1, 2);
        var initialRoom = session.CurrentRoomNumber;

        // Act
        var moved = await _service.MoveToNextRoomAsync(session.Id);

        // Assert
        Assert.True(moved);

        var updatedSession = await _service.GetSessionAsync(session.Id);
        Assert.NotNull(updatedSession);
        Assert.Equal(initialRoom + 1, updatedSession.CurrentRoomNumber);
    }

    [Fact]
    public async Task EndSessionAsync_UpdatesSessionStatus()
    {
        // Arrange
        var session = await _service.StartNewAdventureAsync(1, 2);

        // Act
        var endedSession = await _service.EndSessionAsync(session.Id, GameStatus.Completed);

        // Assert
        Assert.NotNull(endedSession);
        Assert.Equal(GameStatus.Completed, endedSession.Status);
        Assert.NotNull(endedSession.CompletedAt);
    }

    [Fact]
    public async Task GetCurrentRoomAsync_ReturnsCorrectRoom()
    {
        // Arrange
        var session = await _service.StartNewAdventureAsync(1, 2);

        // Act
        var room = await _service.GetCurrentRoomAsync(session.Id);

        // Assert
        Assert.NotNull(room);
        Assert.Equal(1, room.RoomNumber);
        Assert.Equal(session.DungeonId, room.DungeonId);
    }

    [Fact]
    public async Task GetSessionActionsAsync_ReturnsAllActions()
    {
        // Arrange
        var session = await _service.StartNewAdventureAsync(1, 2);
        await _service.PerformActionAsync(session.Id, ActionType.Combat);
        await _service.PerformActionAsync(session.Id, ActionType.Search);

        // Act
        var actions = await _service.GetSessionActionsAsync(session.Id);

        // Assert
        Assert.NotNull(actions);
        Assert.Equal(2, actions.Count);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
