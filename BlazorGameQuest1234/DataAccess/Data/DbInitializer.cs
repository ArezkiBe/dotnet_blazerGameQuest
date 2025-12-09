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
    /// Initialise la base de données avec des données de test.
    /// Appelé automatiquement au démarrage de l'application.
    /// </summary>
    /// <param name="context">Le contexte de base de données</param>
    public static async Task InitializeAsync(GameDbContext context)
    {
        // S'assurer que la base existe et que les migrations sont à jour
        // Stratégie différente selon le type de base : PostgreSQL vs InMemory
        if (!context.Database.IsInMemory())
        {
            // PostgreSQL : appliquer les migrations pendantes
            await context.Database.MigrateAsync();
        }
        else
        {
            // InMemory (tests) : créer le schéma directement
            await context.Database.EnsureCreatedAsync();
        }

        // Pattern Idempotent : vérifier si déjà initialisé pour éviter les doublons
        if (await context.Users.AnyAsync())
        {
            return; // Déjà peuplé, ne rien faire
        }

        // Stratégie d'authentification avec Keycloak :
        // - Les utilisateurs sont gérés par Keycloak (pas de création en base locale)
        // - Les profils Player sont créés automatiquement à la première connexion
        // - Voir PlayersController.GetPlayerByUsername() pour la logique de création automatique

        // Créer un donjon de démonstration (difficulté moyenne : 2/5)
        // En production, les donjons sont générés procéduralement par DungeonGeneratorService
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

        // Créer 5 salles avec difficulté croissante (1 → 5)
        // Pattern roguelike : la difficulté augmente progressivement
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