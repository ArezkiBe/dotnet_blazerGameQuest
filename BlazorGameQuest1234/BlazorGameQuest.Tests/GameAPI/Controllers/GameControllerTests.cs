using GameAPI.Controllers;
using GameAPI.Services;
using DataAccess.Data;
using SharedModels.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace BlazorGameQuest.Tests.GameAPI.Controllers;

/// <summary>
/// Tests unitaires pour le GameController
/// Vérifie les fonctionnalités de gestion des sessions de jeu
/// </summary>
public class GameControllerTests : IDisposable
{
    private readonly GameDbContext _context;
    private readonly GameController _controller;
    private readonly Mock<ILogger<GameController>> _loggerMock;
    private readonly IGameSessionService _gameSessionService;

    public GameControllerTests()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new GameDbContext(options);
        _loggerMock = new Mock<ILogger<GameController>>();
        
        var dungeonGenerator = new DungeonGeneratorService(_context);
        _gameSessionService = new GameSessionService(_context, dungeonGenerator);
        _controller = new GameController(_gameSessionService, _loggerMock.Object);

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
            Score = 100,
            CurrentRoom = 1,
            KeycloakUserId = "kc1",
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        _context.Players.Add(player);
        _context.SaveChanges();
    }

    [Fact]
    public async Task StartNewAdventure_WithValidPlayer_CreatesNewSession()
    {
        // Act
        var result = await _controller.StartNewAdventure(1, 2);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var session = Assert.IsType<GameSession>(okResult.Value);

        Assert.Equal(1, session.PlayerId);
        Assert.Equal(GameStatus.Active, session.Status);
        Assert.Equal(1, session.CurrentRoomNumber);
    }

    [Fact]
    public async Task StartNewAdventure_WithInvalidPlayer_ReturnsNotFound()
    {
        // Act
        var result = await _controller.StartNewAdventure(999, 2);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetGameSession_WithValidSessionId_ReturnsSession()
    {
        // Arrange
        var session = await _gameSessionService.StartNewAdventureAsync(1, 2);

        // Act
        var result = await _controller.GetGameSession(session.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var sessionDetails = okResult.Value;
        Assert.NotNull(sessionDetails);
    }

    [Fact]
    public async Task GetGameSession_WithInvalidSessionId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.GetGameSession(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task EndSession_WithValidSessionId_EndsSession()
    {
        // Arrange
        var session = await _gameSessionService.StartNewAdventureAsync(1, 2);

        // Act
        var result = await _controller.EndSession(session.Id, "Test abandon");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var endedSession = Assert.IsType<GameSession>(okResult.Value);

        Assert.NotNull(endedSession.CompletedAt);
        Assert.Equal(GameStatus.Failed, endedSession.Status);
    }

    [Fact]
    public async Task EndSession_WithInvalidSessionId_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.EndSession(999, "Test");

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
