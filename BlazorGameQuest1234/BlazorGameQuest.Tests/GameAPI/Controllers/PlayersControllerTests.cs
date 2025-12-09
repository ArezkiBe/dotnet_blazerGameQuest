using GameAPI.Controllers;
using DataAccess.Data;
using SharedModels.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace BlazorGameQuest.Tests.GameAPI.Controllers;

/// <summary>
/// Tests unitaires pour le PlayersController
/// Vérifie les opérations CRUD et les fonctionnalités de classement
/// </summary>
public class PlayersControllerTests : IDisposable
{
    private readonly GameDbContext _context;
    private readonly PlayersController _controller;
    private readonly Mock<ILogger<PlayersController>> _loggerMock;

    public PlayersControllerTests()
    {
        // Configuration de la base de données en mémoire
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new GameDbContext(options);
        _loggerMock = new Mock<ILogger<PlayersController>>();
        _controller = new PlayersController(_context, _loggerMock.Object);

        // Initialisation des données de test
        SeedTestData();
    }

    private void SeedTestData()
    {
        var users = new List<User>
        {
            new User { Id = 1, Username = "player1", Email = "player1@test.com", Role = UserRole.Player, IsActive = true, CreatedAt = DateTime.UtcNow },
            new User { Id = 2, Username = "player2", Email = "player2@test.com", Role = UserRole.Player, IsActive = true, CreatedAt = DateTime.UtcNow },
            new User { Id = 3, Username = "player3", Email = "player3@test.com", Role = UserRole.Player, IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        var players = new List<Player>
        {
            new Player { Id = 1, UserId = 1, Username = "player1", Score = 100, CurrentRoom = 1, KeycloakUserId = "kc1", CreatedAt = DateTime.UtcNow },
            new Player { Id = 2, UserId = 2, Username = "player2", Score = 200, CurrentRoom = 2, KeycloakUserId = "kc2", CreatedAt = DateTime.UtcNow },
            new Player { Id = 3, UserId = 3, Username = "player3", Score = 150, CurrentRoom = 1, KeycloakUserId = "kc3", CreatedAt = DateTime.UtcNow }
        };

        _context.Users.AddRange(users);
        _context.Players.AddRange(players);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetPlayers_ReturnsAllPlayers_OrderedByScoreDescending()
    {
        // Act
        var result = await _controller.GetPlayers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var players = Assert.IsAssignableFrom<IEnumerable<Player>>(okResult.Value);
        var playerList = players.ToList();

        Assert.Equal(3, playerList.Count);
        Assert.Equal(200, playerList[0].Score); // player2 a le score le plus élevé
        Assert.Equal(150, playerList[1].Score);
        Assert.Equal(100, playerList[2].Score);
    }

    [Fact]
    public async Task GetPlayer_WithValidId_ReturnsPlayer()
    {
        // Act
        var result = await _controller.GetPlayer(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var player = Assert.IsType<Player>(okResult.Value);
        Assert.Equal(1, player.Id);
        Assert.Equal("player1", player.Username);
    }

    [Fact]
    public async Task GetPlayer_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.GetPlayer(999);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetPlayerByUserId_WithValidUserId_ReturnsPlayer()
    {
        // Act
        var result = await _controller.GetPlayerByUserId(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var player = Assert.IsType<Player>(okResult.Value);
        Assert.Equal(1, player.UserId);
        Assert.Equal("player1", player.Username);
    }

    [Fact]
    public async Task GetPlayerByUserId_WithInvalidUserId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.GetPlayerByUserId(999);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetLeaderboard_ReturnsTop10Players()
    {
        // Act
        var result = await _controller.GetLeaderboard();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var leaderboard = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
        var leaderboardList = leaderboard.ToList();

        Assert.True(leaderboardList.Count <= 10);
        Assert.Equal(3, leaderboardList.Count); // On a 3 joueurs dans les données de test
    }

    [Fact]
    public async Task GetPlayerSessions_WithValidPlayerId_ReturnsSessions()
    {
        // Arrange
        var session = new GameSession
        {
            Id = 1,
            PlayerId = 1,
            DungeonId = 1,
            CurrentRoomNumber = 1,
            Status = GameStatus.Active,
            TotalScore = 50,
            StartedAt = DateTime.UtcNow
        };
        _context.GameSessions.Add(session);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetPlayerSessions(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var sessions = Assert.IsAssignableFrom<IEnumerable<GameSession>>(okResult.Value);
        Assert.Single(sessions);
    }

    [Fact]
    public async Task GetPlayerSessions_WithInvalidPlayerId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.GetPlayerSessions(999);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
