/// <summary>
/// Classe de données initiales pour BlazorGameQuest Version 2
/// Peuple la base de données avec des données de test
/// </summary>
using Microsoft.EntityFrameworkCore;
using SharedModels.Models;

namespace DataAccess.Data;

/// <summary>
/// Classe pour l'initialisation des données de la base de données
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Initialise la base de données avec des données de test
    /// </summary>
    /// <param name="context">Le contexte de base de données</param>
    public static async Task InitializeAsync(GameDbContext context)
    {
        // S'assurer que la base de données existe et que les migrations sont appliquées
        // Uniquement pour les bases de données relationnelles (pas InMemory)
        if (!context.Database.IsInMemory())
        {
            await context.Database.MigrateAsync();
        }
        else
        {
            await context.Database.EnsureCreatedAsync();
        }

        // Vérifier si des données existent déjà
        if (await context.Users.AnyAsync())
        {
            return; // La base de données contient déjà des données
        }

        // Créer des utilisateurs de test
        // Les utilisateurs sont maintenant gérés par Keycloak
        // Plus besoin de créer des utilisateurs de test

        // Les joueurs sont maintenant créés automatiquement lors de leur première connexion
        // via PlayersController.GetPlayerByUsername()

        // Créer un donjon de test
        var dungeon = new Dungeon
        {
            Name = "Château des Ombres",
            Description = "Un ancien château hanté par des créatures mystérieuses",
            DifficultyLevel = 2,
            TotalRooms = 5,
            GeneratedAt = DateTime.UtcNow
        };

        await context.Dungeons.AddAsync(dungeon);
        await context.SaveChangesAsync();

        // Créer les salles du donjon
        var rooms = new[]
        {
            new Room
            {
                DungeonId = dungeon.Id,
                RoomNumber = 1,
                Title = "Hall d'Entrée",
                Description = "Un vaste hall aux murs ornés de tapisseries déchirées",
                EncounterType = "Gobelin",
                Difficulty = 1
            },
            new Room
            {
                DungeonId = dungeon.Id,
                RoomNumber = 2,
                Title = "Bibliothèque Abandonnée",
                Description = "Des étagères remplies de livres poussiéreux et de parchemins anciens",
                EncounterType = "Spectre",
                Difficulty = 2
            },
            new Room
            {
                DungeonId = dungeon.Id,
                RoomNumber = 3,
                Title = "Salle du Trône",
                Description = "Un trône de pierre surmonté d'une couronne brisée",
                EncounterType = "Garde Squelette",
                Difficulty = 3
            },
            new Room
            {
                DungeonId = dungeon.Id,
                RoomNumber = 4,
                Title = "Cachots Profonds",
                Description = "Des cellules sombres résonnent d'échos inquiétants",
                EncounterType = "Troll",
                Difficulty = 4
            },
            new Room
            {
                DungeonId = dungeon.Id,
                RoomNumber = 5,
                Title = "Chambre du Dragon",
                Description = "Une immense caverne où repose un dragon endormi",
                EncounterType = "Dragon Ancien",
                Difficulty = 5
            }
        };

        await context.Rooms.AddRangeAsync(rooms);
        await context.SaveChangesAsync();
    }
}