using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using GameAPI.Controllers;
using DataAccess.Data;
using SharedModels.Models;

namespace BlazorGameQuest.Tests.Controllers;

/// <summary>
/// Tests unitaires pour les contrôleurs essentiels
/// </summary>
public class ControllersTests : IDisposable
{
    private readonly GameDbContext _context;

    public ControllersTests()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new GameDbContext(options);
    }

    [Fact]
    public async Task UsersController_GetUsers_RetourneTousLesUtilisateurs()
    {
        // Arrange
        var controller = new UsersController(_context, new NullLogger<UsersController>());
        var user1 = new User { Username = "User1", Email = "user1@test.com", Role = UserRole.Player };
        var user2 = new User { Username = "User2", Email = "user2@test.com", Role = UserRole.Administrator };
        _context.Users.AddRange(user1, user2);
        await _context.SaveChangesAsync();

        // Act
        var result = await controller.GetUsers();

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<User>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var users = Assert.IsAssignableFrom<IEnumerable<User>>(okResult.Value);
        Assert.Equal(2, users.Count());
    }

    [Fact]
    public async Task UsersController_CreateUser_CreeUtilisateur()
    {
        // Arrange
        var controller = new UsersController(_context, new NullLogger<UsersController>());
        var newUser = new User 
        { 
            Username = "NewUser", 
            Email = "newuser@test.com", 
            Role = UserRole.Player 
        };

        // Act
        var result = await controller.CreateUser(newUser);

        // Assert
        var actionResult = Assert.IsType<ActionResult<User>>(result);
        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var user = Assert.IsType<User>(createdResult.Value);
        Assert.Equal(newUser.Username, user.Username);
    }

    [Fact]
    public async Task PlayersController_GetPlayers_RetourneTousLesJoueurs()
    {
        // Arrange
        var controller = new PlayersController(_context, new NullLogger<PlayersController>());
        var player1 = new Player { Username = "Player1", Score = 100 };
        var player2 = new Player { Username = "Player2", Score = 200 };
        _context.Players.AddRange(player1, player2);
        await _context.SaveChangesAsync();

        // Act
        var result = await controller.GetPlayers();

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<Player>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var players = Assert.IsAssignableFrom<IEnumerable<Player>>(okResult.Value);
        Assert.Equal(2, players.Count());
    }

    [Fact]
    public async Task DungeonsController_GetDungeons_RetourneTousLesDonjons()
    {
        // Arrange
        var controller = new DungeonsController(_context);
        var dungeon1 = new Dungeon { Name = "Donjon Test 1", DifficultyLevel = 1, TotalRooms = 3 };
        var dungeon2 = new Dungeon { Name = "Donjon Test 2", DifficultyLevel = 2, TotalRooms = 5 };
        _context.Dungeons.AddRange(dungeon1, dungeon2);
        await _context.SaveChangesAsync();

        // Act
        var result = await controller.GetDungeons();

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<Dungeon>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dungeons = Assert.IsAssignableFrom<IEnumerable<Dungeon>>(okResult.Value);
        Assert.Equal(2, dungeons.Count());
    }

    [Fact]
    public async Task RoomsController_GetRooms_RetourneToutesLesSalles()
    {
        // Arrange
        var controller = new RoomsController(_context);
        var room1 = new Room { Title = "Salle Test 1", RoomNumber = 1, DungeonId = 1 };
        var room2 = new Room { Title = "Salle Test 2", RoomNumber = 2, DungeonId = 1 };
        _context.Rooms.AddRange(room1, room2);
        await _context.SaveChangesAsync();

        // Act
        var result = await controller.GetRooms();

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<Room>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var rooms = Assert.IsAssignableFrom<IEnumerable<Room>>(okResult.Value);
        Assert.Equal(2, rooms.Count());
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}