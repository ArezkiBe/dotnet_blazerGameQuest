using GameAPI.Controllers;
using DataAccess.Data;
using SharedModels.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace BlazorGameQuest.Tests.GameAPI.Controllers;

/// <summary>
/// Tests unitaires pour l'AdminController
/// Vérifie les fonctionnalités réservées aux administrateurs
/// </summary>
public class AdminControllerTests : IDisposable
{
    private readonly GameDbContext _context;
    private readonly AdminController _controller;
    private readonly Mock<ILogger<AdminController>> _loggerMock;

    public AdminControllerTests()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new GameDbContext(options);
        _loggerMock = new Mock<ILogger<AdminController>>();
        _controller = new AdminController(_context, _loggerMock.Object);

        SeedTestData();
        SetupAdminUser();
    }

    private void SeedTestData()
    {
        var users = new List<User>
        {
            new User { Id = 1, Username = "admin", Email = "admin@test.com", Role = UserRole.Administrator, IsActive = true, CreatedAt = DateTime.UtcNow },
            new User { Id = 2, Username = "user1", Email = "user1@test.com", Role = UserRole.Player, IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        var players = new List<Player>
        {
            new Player { Id = 1, UserId = 2, Username = "user1", Score = 100, CurrentRoom = 1, KeycloakUserId = "kc1", CreatedAt = DateTime.UtcNow }
        };

        _context.Users.AddRange(users);
        _context.Players.AddRange(players);
        _context.SaveChanges();
    }

    private void SetupAdminUser()
    {
        var claims = new List<Claim>
        {
            new Claim("preferred_username", "admin"),
            new Claim(ClaimTypes.Role, "admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task GetDashboardData_AsAdmin_ReturnsStats()
    {
        // Act
        var result = await _controller.GetDashboardData();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(okResult.Value);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
