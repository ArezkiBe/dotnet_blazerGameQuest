using SharedModels.Models;

namespace BlazorGameQuest.Tests;

public class Version2Tests
{
    [Fact]
    public void User_CanBeCreated()
    {
        var user = new User
        {
            Username = "TestUser",
            Email = "test@example.com",
            Role = UserRole.Player
        };

        Assert.Equal("TestUser", user.Username);
        Assert.Equal("test@example.com", user.Email);
        Assert.Equal(UserRole.Player, user.Role);
    }

    [Fact]
    public void Player_CanBeCreated()
    {
        var player = new Player
        {
            UserId = 1,
            Username = "TestPlayer",
            Score = 100
        };

        Assert.Equal(1, player.UserId);
        Assert.Equal("TestPlayer", player.Username);
        Assert.Equal(100, player.Score);
    }

    [Fact]
    public void Dungeon_CanBeCreated()
    {
        var dungeon = new Dungeon
        {
            Name = "Test Dungeon",
            Description = "A test dungeon"
        };

        Assert.Equal("Test Dungeon", dungeon.Name);
        Assert.Equal("A test dungeon", dungeon.Description);
    }

    [Fact]
    public void Room_CanBeCreated()
    {
        var room = new Room
        {
            DungeonId = 1,
            Description = "A test room"
        };

        Assert.Equal(1, room.DungeonId);
        Assert.Equal("A test room", room.Description);
    }

    [Fact]
    public void Version2_ModelsAreValid()
    {
        // Test simple pour vérifier que tous les modèles Version 2 fonctionnent
        var user = new User();
        var player = new Player();
        var dungeon = new Dungeon();
        var room = new Room();

        Assert.NotNull(user);
        Assert.NotNull(player);
        Assert.NotNull(dungeon);
        Assert.NotNull(room);
    }
}