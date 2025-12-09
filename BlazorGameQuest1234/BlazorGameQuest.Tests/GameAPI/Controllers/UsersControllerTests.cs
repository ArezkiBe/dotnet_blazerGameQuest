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
/// Tests unitaires pour le UsersController
/// Vérifie la gestion des utilisateurs et les permissions admin
/// </summary>
public class UsersControllerTests : IDisposable
{
    private readonly GameDbContext _context;
    private readonly UsersController _controller;
    private readonly Mock<ILogger<UsersController>> _loggerMock;

    public UsersControllerTests()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new GameDbContext(options);
        _loggerMock = new Mock<ILogger<UsersController>>();
        _controller = new UsersController(_context, _loggerMock.Object);

        SeedTestData();
        SetupAdminUser();
    }

    private void SeedTestData()
    {
        var users = new List<User>
        {
            new User { Id = 1, Username = "admin", Email = "admin@test.com", Role = UserRole.Administrator, IsActive = true, CreatedAt = DateTime.UtcNow },
            new User { Id = 2, Username = "user1", Email = "user1@test.com", Role = UserRole.Player, IsActive = true, CreatedAt = DateTime.UtcNow },
            new User { Id = 3, Username = "user2", Email = "user2@test.com", Role = UserRole.Player, IsActive = false, CreatedAt = DateTime.UtcNow }
        };

        _context.Users.AddRange(users);
        _context.SaveChanges();
    }

    private void SetupAdminUser()
    {
        var claims = new List<Claim>
        {
            new Claim("preferred_username", "admin"),
            new Claim(ClaimTypes.Role, "administrateur")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task GetUsers_AsAdmin_ReturnsAllUsers()
    {
        // Act
        var result = await _controller.GetUsers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var users = Assert.IsAssignableFrom<IEnumerable<User>>(okResult.Value);
        Assert.Equal(3, users.Count());
    }

    [Fact]
    public async Task GetUser_WithValidId_ReturnsUser()
    {
        // Act
        var result = await _controller.GetUser(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var user = Assert.IsType<User>(okResult.Value);
        Assert.Equal(1, user.Id);
        Assert.Equal("admin", user.Username);
    }

    [Fact]
    public async Task GetUser_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.GetUser(999);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetUserByUsername_WithValidUsername_ReturnsUser()
    {
        // Act
        var result = await _controller.GetUserByUsername("user1");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var user = Assert.IsType<User>(okResult.Value);
        Assert.Equal("user1", user.Username);
    }

    [Fact]
    public async Task GetUserByUsername_WithInvalidUsername_ReturnsNotFound()
    {
        // Act
        var result = await _controller.GetUserByUsername("nonexistent");

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task UpdateUserStatus_WithValidId_UpdatesStatus()
    {
        // Arrange
        var userId = 2;
        var newStatus = false;

        // Act
        var result = await _controller.UpdateUserStatus(userId, newStatus);

        // Assert
        Assert.IsType<OkObjectResult>(result);

        var user = await _context.Users.FindAsync(userId);
        Assert.NotNull(user);
        Assert.Equal(newStatus, user.IsActive);
    }

    [Fact]
    public async Task UpdateUserStatus_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.UpdateUserStatus(999, false);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetUserStatistics_ReturnsCorrectStats()
    {
        // Act
        var result = await _controller.GetUserStatistics();

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
