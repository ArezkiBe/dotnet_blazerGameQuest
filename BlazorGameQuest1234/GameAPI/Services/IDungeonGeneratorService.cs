using SharedModels.Models;

namespace GameAPI.Services;

/// <summary>
/// Service pour la génération aléatoire de donjons
/// </summary>
public interface IDungeonGeneratorService
{
    /// <summary>
    /// Génère un nouveau donjon avec des salles aléatoires
    /// </summary>
    /// <param name="difficultyLevel">Niveau de difficulté global (1-5)</param>
    /// <param name="roomCount">Nombre de salles à générer (1-5, défaut: 5)</param>
    /// <returns>Le donjon généré avec toutes ses salles</returns>
    Task<Dungeon> GenerateDungeonAsync(int difficultyLevel = 2, int roomCount = 5);
    
    /// <summary>
    /// Génère une salle aléatoire pour un donjon
    /// </summary>
    /// <param name="dungeonId">ID du donjon</param>
    /// <param name="roomNumber">Numéro de la salle (1-5)</param>
    /// <param name="baseDifficulty">Difficulté de base du donjon</param>
    /// <returns>La salle générée</returns>
    Room GenerateRandomRoom(int dungeonId, int roomNumber, int baseDifficulty);
}