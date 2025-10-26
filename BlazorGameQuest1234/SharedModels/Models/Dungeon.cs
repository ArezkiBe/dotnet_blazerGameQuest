/// <summary>
/// Donjon généré contenant jusqu'à 5 salles
/// </summary>
namespace SharedModels.Models;

/// <summary>
/// Donjon généré contenant jusqu'à 5 salles
/// </summary>
public class Dungeon
{
    /// <summary>
    /// Identifiant unique du donjon
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Nom/Titre du donjon
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Description générale du donjon
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Niveau de difficulté global (1-5)
    /// </summary>
    public int DifficultyLevel { get; set; } = 1;
    
    /// <summary>
    /// Nombre total de salles (maximum 5)
    /// </summary>
    public int TotalRooms { get; set; } = 5;
    
    /// <summary>
    /// Date de génération du donjon
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}