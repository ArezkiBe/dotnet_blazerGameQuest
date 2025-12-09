using SharedModels.Models;
using DataAccess.Data;

namespace GameAPI.Services;

/// <summary>
/// Service pour la génération aléatoire de donjons
/// Implémente des algorithmes pour créer des salles variées et équilibrées
/// </summary>
public class DungeonGeneratorService : IDungeonGeneratorService
{
    private readonly GameDbContext _context;
    private readonly Random _random;

    // Templates de salles pour la génération aléatoire avec types spécifiques
    private readonly List<RoomTemplate> _roomTemplates = new()
    {
        // Salles de monstres
        new("Hall d'Entrée", "Un vaste hall aux murs ornés de tapisseries déchirées", "Gobelin", RoomType.Monster),
        new("Crypte Sombre", "Des sarcophages anciens dans une atmosphère lugubre", "Zombie", RoomType.Monster),
        new("Forge Abandonnée", "Un atelier de forgeron avec des braises encore chaudes", "Élémentaire de Feu", RoomType.Monster),
        
        // Salles de trésor
        new("Chambre du Trésor", "Une salle brillante remplie d'or et de gemmes", "Coffres Enchantés", RoomType.Treasure),
        new("Bibliothèque Ancienne", "Des étagères remplies de livres précieux et parchemins", "Coffre Secret", RoomType.Treasure),
        
        // Salles piégées
        new("Laboratoire d'Alchimie", "Des fioles bouillonnantes et des mécanismes dangereux", "Pièges Alchimiques", RoomType.Trap),
        new("Salle des Miroirs", "Des miroirs magiques aux reflets trompeurs", "Illusions Piégées", RoomType.Trap),
        
        // Salles vides
        new("Prison Souterraine", "Des cellules rouillées avec des objets éparpillés", "Débris", RoomType.Empty),
        
        // Salles de repos
        new("Fontaine Sacrée", "Une fontaine cristalline aux eaux curatives", "Source Magique", RoomType.Rest),
        
        // Salles mystérieuses
        new("Salle du Trône Maudit", "Un trône de pierre entouré d'une aura sombre", "Artefact Mystérieux", RoomType.Mystery),
        new("Jardin Enchanté", "Un jardin souterrain aux plantes étranges", "Magie Sauvage", RoomType.Mystery)
    };

    public DungeonGeneratorService(GameDbContext context)
    {
        _context = context;
        _random = new Random();
    }

    /// <summary>
    /// Génère un nouveau donjon avec des salles aléatoires
    /// </summary>
    public async Task<Dungeon> GenerateDungeonAsync(int difficultyLevel = 2, int roomCount = 5)
    {
        // Valider les paramètres
        difficultyLevel = Math.Clamp(difficultyLevel, 1, 5);
        roomCount = Math.Clamp(roomCount, 1, 5);

        // Générer le donjon
        var dungeon = new Dungeon
        {
            Name = GenerateRandomDungeonName(),
            Description = GenerateRandomDungeonDescription(),
            DifficultyLevel = difficultyLevel,
            TotalRooms = roomCount,
            GeneratedAt = DateTime.UtcNow
        };

        // Sauvegarder le donjon d'abord pour obtenir l'ID
        _context.Dungeons.Add(dungeon);
        await _context.SaveChangesAsync();

        // Générer les salles
        var rooms = new List<Room>();
        for (int i = 1; i <= roomCount; i++)
        {
            var room = GenerateRandomRoom(dungeon.Id, i, difficultyLevel);
            rooms.Add(room);
        }

        _context.Rooms.AddRange(rooms);
        await _context.SaveChangesAsync();

        return dungeon;
    }

    /// <summary>
    /// Génère une salle aléatoire avec des caractéristiques variées.
    /// La difficulté fluctue autour de baseDifficulty (±1 niveau) pour éviter la monotonie.
    /// </summary>
    public Room GenerateRandomRoom(int dungeonId, int roomNumber, int baseDifficulty)
    {
        var template = _roomTemplates[_random.Next(_roomTemplates.Count)];
        
        // Difficulté variable : baseDifficulty ± 1 (clampée entre 1-5)
        var roomDifficulty = Math.Clamp(baseDifficulty + _random.Next(-1, 2), 1, 5);
        
        var room = new Room
        {
            DungeonId = dungeonId,
            RoomNumber = roomNumber,
            Title = $"Salle {roomNumber} : {template.Title}",
            Description = template.Description,
            Type = template.Type,
            EncounterType = template.EncounterType,
            Difficulty = roomDifficulty
        };

        // Configurer les mécaniques de jeu basées sur la difficulté et le type
        ConfigureRoomMechanics(room, roomDifficulty);

        return room;
    }

    /// <summary>
    /// Configure les mécaniques de jeu d'une salle selon sa difficulté.
    /// Formules : CombatSuccess = max(30, 90-10*diff), CombatReward = 10+5*diff
    /// </summary>
    private void ConfigureRoomMechanics(Room room, int difficulty)
    {
        room.CombatSuccessRate = Math.Max(30, 90 - (difficulty * 10)); // 80%→30% selon diff
        room.CombatReward = 10 + (difficulty * 5); // 15→35 pts
        room.FleeReward = 3 + difficulty; // 4→8 pts (toujours faible)
        room.SearchSuccessRate = 40 + _random.Next(-10, 21); // 30%→60% (variabilité aléatoire)
        room.SearchReward = 8 + (difficulty * 2) + _random.Next(-3, 4);
        room.SearchPenalty = -(2 + difficulty + _random.Next(-1, 3));
    }

    /// <summary>
    /// Génère un nom aléatoire pour le donjon
    /// </summary>
    private string GenerateRandomDungeonName()
    {
        var prefixes = new[] { "Château", "Tour", "Crypte", "Labyrinthe", "Temple", "Forteresse" };
        var suffixes = new[] { "des Ombres", "Maudit", "Oublié", "Éternel", "de la Mort", "du Chaos", "Hanté" };
        
        var prefix = prefixes[_random.Next(prefixes.Length)];
        var suffix = suffixes[_random.Next(suffixes.Length)];
        
        return $"{prefix} {suffix}";
    }

    /// <summary>
    /// Génère une description aléatoire pour le donjon
    /// </summary>
    private string GenerateRandomDungeonDescription()
    {
        var descriptions = new[]
        {
            "Un ancien donjon rempli de mystères et de dangers",
            "Une forteresse abandonnée hantée par des créatures maléfiques",
            "Un labyrinthe souterrain aux passages tortueux",
            "Les ruines d'un temple dédié à des dieux oubliés",
            "Une prison magique où le temps semble suspendu"
        };
        
        return descriptions[_random.Next(descriptions.Length)];
    }

    /// <summary>
    /// Template pour la génération de salles avec types spécifiques
    /// </summary>
    private record RoomTemplate(string Title, string Description, string EncounterType, RoomType Type);
}